/***
C/C++ sandbox for testing a cpp lexer written in C++.
Even though its C++, i am not using any object orientation concepts.

Interestingly, this lexer is 3-4 times faster than the C# version - with the same result.
Even in debug build, its faster than the C# version in release-build O_o
In release mode this lexer requires ~60 ms to parse the "final_platform_layer.h" file - which is 20.000 lines of codes, ~750.000 characters

I am not sure why this is, maybe due to the proper usage of structs here?
***/

// When COPY_TOKEN_VALUES is set, all token values are cloned (Only for debugging purposes, to see each token with proper string termination)
#define COPY_TOKEN_VALUES

// When NO_MEMORY_BLOCKS is set, each memory push is a malloc() with a entry in a pointer vector -> Slow
// When NO_MEMORY_BLOCKS is not set, custom power-of-two aligned memory blocks are allocated, which can be used like a stack -> Fast
//#define NO_MEMORY_BLOCKS

#include <cstdio>
#include <cstdint>
#include <cstdarg>
#include <algorithm>
#include <limits>
#include <chrono>
#include <vector>

#include <assert.h>
#include <malloc.h>

#define WIN32_LEAN_AND_MEAN 1
#define NOMINMAX 1
#include <Windows.h>
#include <shlobj.h>

// ****************************************************************************
// Macros functions
// ****************************************************************************
#define ArrayCount(arr) (sizeof(arr) / sizeof((arr)[0]))

// ****************************************************************************
// Memory-Block
// ****************************************************************************

#if !defined(NO_MEMORY_BLOCKS)
constexpr size_t MinMemoryBlockSize = 4096;

struct MemoryBlockHeader {
	MemoryBlockHeader *prev;
	MemoryBlockHeader *next;
	size_t totalSize;
};

struct MemoryBlock {
	void *data;
	size_t length;
	size_t offset;

private:
	inline uint64_t RoundUpPowerOfTwo(uint64_t v) {
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v |= v >> 32;
		v++;
		return(v);
	}

	inline size_t GetBlockSize(size_t size) {
		size_t minSize = size + sizeof(MemoryBlockHeader);
		size_t result = std::max(RoundUpPowerOfTwo(minSize), MinMemoryBlockSize);
		return(result);
	}

public:
	bool Allocate(size_t initSize) {
		assert(length == 0 && data == nullptr);
		assert(initSize > 0);
		size_t blockSize = GetBlockSize(initSize);
		void *base = malloc(blockSize);
		if (base == nullptr) {
			return(false);
		}
		size_t dataLength = blockSize - sizeof(MemoryBlockHeader);
		MemoryBlockHeader *header = (MemoryBlockHeader *)base;
		*header = {};
		header->totalSize = blockSize;

		data = (uint8_t *)base + sizeof(MemoryBlockHeader);
		length = dataLength;
		offset = 0;
		return(true);
	}

	void *PushSize(size_t pushSize, bool clear = false) {
		// Layout of memory block: [Block-Header0][Data0],[Block-Header1][Data1],...
		MemoryBlockHeader *thisHeader = nullptr;
		if (length > 0) {
			thisHeader = (MemoryBlockHeader *)((uint8_t *)data - sizeof(MemoryBlockHeader));
		}
		if ((length == 0) || (offset + pushSize >= length)) {
			size_t blockSize = GetBlockSize(pushSize);
			void *base = malloc(blockSize);
			if (base == nullptr) {
				return(nullptr);
			}
			size_t dataLength = blockSize - sizeof(MemoryBlockHeader);
			MemoryBlockHeader *header = (MemoryBlockHeader *)base;
			*header = {};
			header->totalSize = blockSize;
			if (thisHeader != nullptr) {
				thisHeader->next = header;
				header->prev = thisHeader;
			}

			data = (uint8_t *)base + sizeof(MemoryBlockHeader);
			length = dataLength;
			offset = 0;

			if (clear) {
				memset(data, 0, length);
			}

			thisHeader = header;
		}
		void *result = (uint8_t *)data + offset;
		offset += pushSize;
		return(result);
	}

	template <typename T>
	T* PushSize(size_t pushSize, bool clear = false) {
		T *result = (T *)PushSize(pushSize, clear);
		return(result);
	}

	const char *PushString(const char *source, size_t len) {
		size_t size = len + 1;
		char *result = (char *)PushSize(size);
		if (result == nullptr) {
			return(nullptr);
		}
		memcpy_s(result, size, source, len);
		result[len] = 0;
		return(result);
	}

	void Release() {
		MemoryBlockHeader *header = nullptr;
		if (length > 0) {
			header = (MemoryBlockHeader *)((uint8_t *)data - sizeof(MemoryBlockHeader));
			assert(header->next == nullptr);
		}
		while (header != nullptr) {
			void *base = header;
			MemoryBlockHeader *prev = header->prev;
			free(base);
			header = prev;
		}
	}
};
#else
struct MemoryBlock {
	std::vector<void *> items;

	void *PushSize(size_t pushSize, bool clear = false) {
		void *base = clear ? calloc(1, pushSize) : malloc(pushSize);
		items.push_back(base);
		return(base);
	}

	template <typename T>
	T* PushSize(size_t pushSize, bool clear = false) {
		T *result = (T *)PushSize(pushSize, clear);
		return(result);
	}

	const char *PushString(const char *source, size_t len) {
		size_t size = (len + 1) * sizeof(char);
		char *base = PushSize<char>(size);
		strncpy_s(base, size, source, len);
		base[len] = 0;
		return(base);
	}
	void Release() {
		for (int i = 0; i < items.size(); ++i) {
			void *base = items.at(i);
			free(base);
		}
		items.clear();
	}
};
#endif

// ****************************************************************************
// String functions
// ****************************************************************************

inline bool IsLineBreak(char c) {
	bool result = (c == '\n') || (c == '\r');
	return(result);
}
inline size_t GetLineBreaks(char first, char second) {
	if ((first == '\r' && second == 'n') || (first == '\n' && second == 'r')) {
		return(2);
	} else {
		return(1);
	}
}
inline bool IsTab(char c) {
	bool result = (c == '\t');
	return(result);
}
inline bool IsSpacing(char c) {
	bool result = (c == '\v') || (c == '\f') || (c == ' ');
	return(result);
}
inline bool IsWhitespace(char c) {
	bool result = IsSpacing(c) || IsTab(c) || IsLineBreak(c);
	return(result);
}
inline bool IsAlpha(char c) {
	bool result = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
	return(result);
}
inline bool IsNumeric(char c) {
	bool result = (c >= '0' && c <= '9');
	return(result);
}
inline bool IsOctal(char c) {
	bool result = (c >= '0' && c <= '7');
	return(result);
}
inline bool IsBinary(char c) {
	bool result = (c == '0' || c == '1');
	return(result);
}
inline bool IsHex(char c) {
	bool result = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
	return(result);
}
inline bool IsIdentStart(char c) {
	bool result = IsAlpha(c) || (c == '_');
	return(result);
}
inline bool IsIdentPart(char c) {
	bool result = IsAlpha(c) || IsNumeric(c) || (c == '_');
	return(result);
}
inline bool IsIntegerSuffix(char c) {
	bool result = (c == 'u' || c == 'U') || (c == 'l' || c == 'L');
	return(result);
}
inline bool IsFloatSuffix(char c) {
	bool result = (c == 'f' || c == 'F') || (c == 'l' || c == 'L');
	return(result);
}
inline bool IsExponentPrefix(char c) {
	bool result = c == 'e' || c == 'E' || c == 'p' || c == 'P';
	return(result);
}
static uint64_t StringHashDJB2(const char *str, size_t maxLen) {
	uint64_t hash = 5381ULL;
	int c;
	int l = 0;
	while ((c = *str++) && (l < maxLen)) {
		hash = ((hash << 5ULL) + hash) + c; /* hash * 33 + c */
		++l;
	}
	return hash;
}

// ****************************************************************************
// List
// ****************************************************************************

template <typename T>
struct List {
	T *items;
	size_t count;
	size_t capacity;
	size_t minCapacity;

	void Add(MemoryBlock *memory, const T &token) {
		if (capacity == 0 || count >= capacity) {
			T *oldItems = items;
			size_t oldCapacity = capacity;
			size_t newCapacity = std::max(capacity * 2, std::max(1ULL, minCapacity));
			size_t newSize = sizeof(T) * newCapacity;
			T *newTokens = memory->PushSize<T>(newSize);
			if (oldItems != nullptr) {
				memcpy_s(newTokens, newSize, oldItems, sizeof(T) * oldCapacity);
			}
			items = newTokens;
			capacity = newCapacity;
		}
		items[count++] = token;
	}
};

// ****************************************************************************
// String-Hashset
// ****************************************************************************
struct StringHashTable {
private:
	typedef struct {
		uint64_t key;
		const char *value;
		size_t valueLen;
		uint64_t unused;
	} entry;
	entry *entries;
	size_t count;
	size_t capacity;
	size_t minCapacity;

	void Add(MemoryBlock *memory, uint64_t key, const char *value, size_t valueLen) {
		if (capacity == 0 || count >= capacity) {
			entry *oldEntries = entries;
			size_t oldCapacity = capacity;
			size_t newCapacity = std::max(capacity * 2, std::max(1ULL, minCapacity));
			size_t newSize = sizeof(entry) * newCapacity;
			entry *newEntries = memory->PushSize<entry>(newSize, true);
			if (oldEntries != nullptr) {
				memcpy_s(newEntries, newSize, oldEntries, sizeof(entry) * oldCapacity);
			}
			entries = newEntries;
			capacity = newCapacity;
		}

		size_t index = key % capacity;
		size_t startIndex = index;
		do {
			entry *entry = entries + index;
			if (entry->key == 0) {
				entry->key = key;
				entry->value = value;
				entry->valueLen = valueLen;
				count++;
				break;
			}
			index = (index + 1) % capacity;
		} while (index != startIndex);
	}
public:

	bool Contains(const char *match, size_t len) {
		if (capacity == 0) 
			return(false);
		uint64_t key = StringHashDJB2(match, len);
		size_t index = key % capacity;
		size_t startIndex = index;
		do {
			entry *entry = entries + index;
			if (entry->key == key) {
				if (strncmp(match, entry->value, len) == 0) {
					return(true);
				}
			}
			index = (index + 1) % capacity;
		} while (index != startIndex);
		return(false);
	}

	void Add(MemoryBlock *memory, const char *value, size_t valueLen) {
		uint64_t key = StringHashDJB2(value, valueLen);
		Add(memory, key, value, valueLen);
	}
};

// ****************************************************************************
// Structs for tokenizer
// ****************************************************************************

struct Text {
	const char *data;
	size_t length;
};

struct TextPosition {
	size_t index;
	size_t line;
	size_t column;
};

typedef bool (BufferedStreamIsCharFunc)(char c);
struct BufferedStream {
	Text text;
	TextPosition textPos;
	const char *remaining;
	size_t columnsPerTab;

private:
	void Advance(size_t count = 1) {
		textPos.index += count;

		// @NOTE(final): Remaining is just for the debugger
		if (textPos.index < text.length) {
			remaining = text.data + textPos.index;
		} else {
			remaining = nullptr;
		}
	}
public:
	bool IsEOF() {
		bool result = (textPos.index >= text.length);
		return(result);
	}

	char Peek(int64_t offset = 0) {
		if ((textPos.index + offset) < text.length) {
			char result = text.data[textPos.index + offset];
			return(result);
		}
		return 0;
	}

	void AdvanceColumn(size_t count = 1) {
		Advance(count);
		textPos.column += count;
	}

	void AdvanceLine(size_t charCount) {
		Advance(charCount);
		textPos.column = 0;
		++textPos.line;
	}

	void AdvanceTab() {
		Advance();
		textPos.column += std::max(columnsPerTab, 1ULL);
	}

	void AdvanceColumnsWhile(BufferedStreamIsCharFunc *readFunc, int maxCount = 0) {
		assert(readFunc(Peek()));
		int count = 0;
		while (!IsEOF()) {
			if (!readFunc(Peek()))
				break;
			if (maxCount > 0 && count >= maxCount)
				break;
			AdvanceColumn();
			count++;
		}
	}

	void SkipWhitespaces() {
		if (IsWhitespace(Peek())) {
			while (!IsEOF()) {
				char first = Peek();
				char second = Peek(1);
				if (IsTab(first)) {
					AdvanceTab();
				} else if (IsLineBreak(first)) {
					size_t lb = GetLineBreaks(first, second);
					AdvanceLine(lb);
				} else if (IsSpacing(first)) {
					AdvanceColumn();
				} else
					break;
			}
		}
	}
};

static StringHashTable globalClassKeywordsHashTable = {};
static StringHashTable globalReservedKeywordsHashTable = {};
static StringHashTable globalTypeKeywordsHashTable = {};

static const char* gReservedKeywords[] = {
	// C99
	"auto",
	"break",
	"case",
	"const",
	"continue",
	"default",
	"do",
	"else",
	"enum",
	"extern",
	"for",
	"goto",
	"if",
	"inline",
	"register",
	"restrict",
	"return",
	"signed",
	"sizeof",
	"static",
	"struct",
	"switch",
	"typedef",
	"union",
	"unsigned",
	"void",
	"volatile",
	"while",
	"_Alignas",
	"_Alignof",
	"__asm__",
	"__volatile__",

	// C++
	"abstract",
	"alignas",
	"alignof",
	"asm",
	"catch",
	"class",
	"constexpr",
	"const_cast",
	"decltype",
	"delete",
	"dynamic_cast",
	"explicit",
	"export",
	"false",
	"friend",
	"mutable",
	"namespace",
	"new",
	"noexcept",
	"nullptr",
	"operator",
	"override",
	"private",
	"protected",
	"public",
	"reinterpret_cast",
	"static_assert",
	"static_cast",
	"template",
	"this",
	"thread_local",
	"throw",
	"try",
	"typeid",
	"typename",
	"virtual",
};

static const char* gTypeKeywords[] = {
	// C99
	"char",
	"double",
	"float",
	"int",
	"long",
	"short",
	"_Bool",
	"_Complex",
	"_Imaginary",

	// C++
	"bool",
	"complex",
	"imaginary",
};

static const char* gGlobalClassKeywords[] = {
	"NULL",
	"int8_t",
	"int16_t",
	"int32_t",
	"int64_t",
	"intptr_t",
	"offset_t",
	"size_t",
	"ssize_t",
	"time_t",
	"uint8_t",
	"uint16_t",
	"uint32_t",
	"uint64_t",
	"uintptr_t",
	"wchar_t",
};

enum class TokenKind {
	Unknown = -1,

	Eof = 0,
	Spacings,
	EndOfLine,

	SingleLineComment,
	SingleLineCommentDoc,
	MultiLineComment,
	MultiLineCommentDoc,

	Preprocessor,

	IdentLiteral,
	ReservedKeyword,
	TypeKeyword,

	StringLiteral,
	CharLiteral,
	IntegerLiteral,
	HexLiteral,
	OctalLiteral,
	BinaryLiteral,
	IntegerFloatLiteral,
	HexadecimalFloatLiteral,

	RightShiftAssign,
	LeftShiftAssign,
	AddAssign,
	SubAssign,
	MulAssign,
	DivAssign,
	ModAssign,
	AndAssign,
	OrAssign,
	XorAssign,
	RightShiftOp,
	LeftShiftOp,
	IncOp,
	DecOp,
	PtrOp,
	LogicalAndOp,
	LogicalOrOp,
	LessOrEqualOp,
	GreaterOrEqualOp,
	LogicalEqualsOp,
	LogicalNotEqualsOp,

	EqOp,
	AndOp,
	OrOp,
	XorOp,
	AddOp,
	SubOp,
	MulOp,
	DivOp,
	ModOp,
	LessThanOp,
	GreaterThanOp,

	LeftParen,
	RightParen,
	LeftBrace,
	RightBrace,
	LeftBracket,
	RightBracket,

	Ellipsis,
	ExclationMark,
	QuestionMark,
	Dot,
	Raute,
	Backslash,
	Tilde,
	Semicolon,
	Comma,
	Colon,
};

static const char *GetTokenKindName(TokenKind kind) {
	switch (kind) {
		case TokenKind::Unknown: return "Unknown";

		case TokenKind::Eof: return "Eof";
		case TokenKind::Spacings: return "Spacings";
		case TokenKind::EndOfLine: return "EndOfLine";

		case TokenKind::MultiLineComment: return "MultiLineComment";
		case TokenKind::MultiLineCommentDoc: return "MultiLineCommentDoc";
		case TokenKind::SingleLineComment: return "SingleLineComment";
		case TokenKind::SingleLineCommentDoc: return "SingleLineCommentDoc";

		case TokenKind::IdentLiteral: return "IdentLiteral";
		case TokenKind::ReservedKeyword: return "ReservedKeyword";
		case TokenKind::TypeKeyword: return "TypeKeyword";

		case TokenKind::StringLiteral: return "StringLiteral";
		case TokenKind::CharLiteral: return "CharLiteral";
		case TokenKind::IntegerLiteral: return "IntegerLiteral";
		case TokenKind::HexLiteral: return "HexLiteral";
		case TokenKind::OctalLiteral: return "OctalLiteral";
		case TokenKind::BinaryLiteral: return "BinaryLiteral";

		case TokenKind::IntegerFloatLiteral: return "IntegerFloatLiteral";
		case TokenKind::HexadecimalFloatLiteral: return "HexadecimalFloatLiteral";

		case TokenKind::EqOp: return "EqOp";

		case TokenKind::LogicalEqualsOp: return "LogicalEqualsOp";
		case TokenKind::LogicalNotEqualsOp: return "LogicalNotEqualsOp";

		case TokenKind::AndAssign: return "AndAssign";
		case TokenKind::LogicalAndOp: return "LogicalAndOp";
		case TokenKind::AndOp: return "AndOp";

		case TokenKind::OrAssign: return "OrAssign";
		case TokenKind::LogicalOrOp: return "LogicalOrOp";
		case TokenKind::OrOp: return "OrOp";

		case TokenKind::XorAssign: return "XorAssign";
		case TokenKind::XorOp: return "XorOp";

		case TokenKind::ModAssign: return "ModAssign";
		case TokenKind::ModOp: return "ModOp";

		case TokenKind::DivOp: return "DivOp";
		case TokenKind::DivAssign: return "DivAssign";

		case TokenKind::MulOp: return "MulOp";
		case TokenKind::MulAssign: return "MulAssign";

		case TokenKind::IncOp: return "IncOp";
		case TokenKind::AddOp: return "AddOp";
		case TokenKind::AddAssign: return "AddAssign";

		case TokenKind::DecOp: return "DecOp";
		case TokenKind::SubOp: return "SubOp";
		case TokenKind::PtrOp: return "PtrOp";
		case TokenKind::SubAssign: return "SubAssign";

		case TokenKind::LeftShiftAssign: return "LeftShiftAssign";
		case TokenKind::LeftShiftOp: return "LeftShiftOp";
		case TokenKind::LessOrEqualOp: return "LessOrEqualOp";
		case TokenKind::LessThanOp: return "LessThanOp";

		case TokenKind::RightShiftAssign: return "RightShiftAssign";
		case TokenKind::RightShiftOp: return "RightShiftOp";
		case TokenKind::GreaterOrEqualOp: return "GreaterOrEqualOp";
		case TokenKind::GreaterThanOp: return "GreaterThanOp";

		case TokenKind::LeftParen: return "LeftParen";
		case TokenKind::RightParen: return "RightParen";
		case TokenKind::LeftBrace: return "LeftBrace";
		case TokenKind::RightBrace: return "RightBrace";
		case TokenKind::LeftBracket: return "LeftBracket";
		case TokenKind::RightBracket: return "RightBracket";

		case TokenKind::Ellipsis: return "Ellipsis";
		case TokenKind::Tilde: return "Tilde";
		case TokenKind::Raute: return "Raute";
		case TokenKind::ExclationMark: return "ExclationMark";
		case TokenKind::QuestionMark: return "QuestionMark";
		case TokenKind::Colon: return "Colon";
		case TokenKind::Semicolon: return "Semicolon";
		case TokenKind::Comma: return "Comma";
		case TokenKind::Dot: return "Dot";
		case TokenKind::Backslash: return "Backslash";

		default:
			return "Unknown";
	}
}

struct Token {
	TextPosition position;
	const char *value;
	size_t length;
	TokenKind kind;
	bool isComplete;
};

struct Timespan {
	double totalSeconds;
	double totalMilliseconds;
};

struct Stopwatch {
	std::chrono::time_point<std::chrono::steady_clock> start;
	std::chrono::time_point<std::chrono::steady_clock> end;
	bool started;
	Timespan elapsed;

	static Stopwatch StartNow() {
		Stopwatch result = {};
		result.started = true;
		result.start = std::chrono::high_resolution_clock::now();
		result.end = result.start;
		return(result);
	}

	void Restart() {
		started = true;
		start = std::chrono::high_resolution_clock::now();
		end = start;
		elapsed = {};
	}

	void Stop() {
		if (started) {
			started = false;
			end = std::chrono::high_resolution_clock::now();
			elapsed.totalMilliseconds = std::chrono::duration<double, std::milli>(end - start).count();
			elapsed.totalSeconds = std::chrono::duration<double>(end - start).count();
		}
	}
};

struct TokenResult {
	TokenKind kind;
	bool isComplete;
};

struct TokenError {
	TextPosition pos;
	const char *message;
};

struct Tokenizer {
	List<Token> tokens;
	List<TokenError> errors;
	MemoryBlock *memory;
	BufferedStream *stream;
	TextPosition lexemeStart;

	void PushError(const char *message, ...) {
		va_list argList;
		va_start(argList, message);
		const size_t bufferSize = 1024 + 1;
		char messageFormat[bufferSize];
		int count = vsnprintf_s(messageFormat, bufferSize, bufferSize, message, argList);
		va_end(argList);

		const size_t errorBufferSize = bufferSize + 128 + 1;
		char errorBuffer[errorBufferSize];
		memcpy_s(errorBuffer, errorBufferSize, messageFormat, count);
		char *s = errorBuffer;
		s += count;
		size_t remainingLen = s - errorBuffer;
		int i = sprintf_s(s, remainingLen, " on (line: %zu, col: %zu, pos: %zu)", stream->textPos.line, stream->textPos.column, stream->textPos.index);
		s += i;

		size_t outLen = s - errorBuffer;

		const char *outstring = memory->PushString(errorBuffer, outLen);

		TokenError error = {};
		error.pos = stream->textPos;
		error.message = outstring;
		errors.Add(memory, error);
	}

	void PushToken(TokenKind kind, TextPosition startPos, TextPosition endPos, bool isComplete) {
		size_t tokenLen = endPos.index - startPos.index;
		const char *source = stream->text.data + startPos.index;
		Token token = {};

#if defined(COPY_TOKEN_VALUES)
		token.value = memory->PushString(source, tokenLen);
#else
		token.value = source;
#endif

		token.kind = kind;
		token.position = startPos;
		token.length = tokenLen;
		token.isComplete = isComplete;
		tokens.Add(memory, token);
	}

	void PushLexeme(TokenKind kind, bool isComplete) {
		TextPosition startPos = lexemeStart;
		TextPosition endPos = stream->textPos;
		PushToken(kind, startPos, endPos, isComplete);
	}

	void StartLexeme() {
		lexemeStart = stream->textPos;
	}
};



static TokenResult LexMultiLineComment(Tokenizer &tokenizer) {
	BufferedStream *stream = tokenizer.stream;
	assert(stream->Peek() == '/');
	assert(stream->Peek(1) == '*');
	stream->AdvanceColumn(2);

	TokenResult result = {};
	result.kind = TokenKind::MultiLineComment;

	char n = stream->Peek();
	if (n == '*' || n == '!') {
		result.kind = TokenKind::MultiLineCommentDoc;
		stream->AdvanceColumn();
	}

	while (!stream->IsEOF()) {
		char first = stream->Peek();
		char second = stream->Peek(1);
		if (first == '*' && second == '/') {
			stream->AdvanceColumn(2);
			result.isComplete = true;
			break;
		} else if (IsLineBreak(first)) {
			size_t nb = GetLineBreaks(first, second);
			stream->AdvanceLine(nb);
		} else if (IsTab(first))
			stream->AdvanceTab();
		else
			stream->AdvanceColumn();
	}
	return(result);
}

static TokenResult LexSingleLineComment(Tokenizer &tokenizer) {
	BufferedStream *stream = tokenizer.stream;
	assert(stream->Peek() == '/');
	assert(stream->Peek(1) == '/');
	stream->AdvanceColumn(2);

	TokenResult result = {};
	result.kind = TokenKind::SingleLineComment;

	char n = stream->Peek();
	if (n == '/' || n == '!') {
		result.kind = TokenKind::SingleLineCommentDoc;
		stream->AdvanceColumn();
	}

	while (!stream->IsEOF()) {
		char first = stream->Peek();
		if (IsLineBreak(first)) {
			result.isComplete = true;
			break;
		} else if (IsTab(first))
			stream->AdvanceTab();
		else
			stream->AdvanceColumn();
	}
	return(result);
}


static TokenResult LexIdent(Tokenizer &tokenizer) {
	BufferedStream *stream = tokenizer.stream;
	assert(IsIdentStart(stream->Peek()));
	TokenResult result = {};
	result.isComplete = true;
	result.kind = TokenKind::IdentLiteral;
	stream->AdvanceColumnsWhile(IsIdentPart);
	const char *source = stream->text.data + tokenizer.lexemeStart.index;
	size_t len = stream->textPos.index - tokenizer.lexemeStart.index;
	if (globalReservedKeywordsHashTable.Contains(source, len)) {
		result.kind = TokenKind::ReservedKeyword;
	} else if (globalClassKeywordsHashTable.Contains(source, len) || globalTypeKeywordsHashTable.Contains(source, len)) {
		result.kind = TokenKind::TypeKeyword;
	}
	return(result);
}

static bool LexExponent(Tokenizer &tokenizer) {
	BufferedStream *stream = tokenizer.stream;
	char c = stream->Peek();
	// Exponent start
	assert(c == 'e' || c == 'E' || c == 'p' || c == 'P');
	stream->AdvanceColumn();

	// Optional exponent-sign
	c = stream->Peek();
	if (c == '+' || c == '-') {
		stream->AdvanceColumn();
	}

	// Digit-sequence
	if (!IsNumeric(stream->Peek())) {
		tokenizer.PushError("Expect digit-sequence for exponent value, but got '%c'", stream->Peek());
		return(false);
	}
	stream->AdvanceColumnsWhile(IsNumeric);
	return(true);
}

static TokenResult LexNumber(Tokenizer &tokenizer) {
	// @NOTE(final): Limitations:
	// - We dont support parentheses grouping of c++ integer literals, such as (0xE)+2.0
	// - We dont support any C++ hexadecimal exponent literals, which ends with E or e
	// - We dont support any C++ user-defined literals
	// - We dont support any C++ escape characters for number literals
	BufferedStream *stream = tokenizer.stream;
	char first = stream->Peek();
	char second = stream->Peek(1);
	assert(IsNumeric(first) || first == '.');

	TokenResult result = {};
	result.isComplete = true;

	bool dotSeen = false;

	if (first == '0') {
		if (second == 'x' || second == 'X') {
			result.kind = TokenKind::HexLiteral;
			stream->AdvanceColumn(2);
		} else if (second == 'b' || second == 'B') {
			result.kind = TokenKind::BinaryLiteral;
			stream->AdvanceColumn(2);
		} else {
			result.kind = TokenKind::OctalLiteral;
		}
	} else if (first == '.') {
		assert(IsNumeric(second));
		result.kind = TokenKind::IntegerFloatLiteral;
		stream->AdvanceColumn(1);
		dotSeen = true;
	} else {
		assert(IsNumeric(first) && first != '0');
		result.kind = TokenKind::IntegerLiteral;
	}

	// @NOTE(final): We never set the DecimalHexLiteral kind initially,
	// as every hex decimal always starts as a normal hex literal!
	assert(result.kind != TokenKind::HexadecimalFloatLiteral);

	size_t firstLiteralPos = stream->textPos.index;
	bool readNextLiteral = false;
	do {
		readNextLiteral = false;
		size_t s = stream->textPos.index;
		switch (result.kind) {
			case TokenKind::IntegerLiteral:
			case TokenKind::IntegerFloatLiteral:
				if (IsNumeric(stream->Peek())) {
					stream->AdvanceColumnsWhile(IsNumeric);
					break;
				} else {
					tokenizer.PushError("Expect integer literal, but got '%c'", stream->Peek());
					return(result);
				}
			case TokenKind::OctalLiteral:
				if (IsOctal(stream->Peek())) {
					stream->AdvanceColumnsWhile(IsOctal);
					break;
				} else {
					tokenizer.PushError("Expect octal literal, but got '%c'", stream->Peek());
					return(result);
				}
			case TokenKind::HexLiteral:
				if (IsHex(stream->Peek())) {
					stream->AdvanceColumnsWhile(IsHex);
					break;
				} else {
					tokenizer.PushError("Expect hex literal, but got '%c'", stream->Peek());
					return(result);
				}
			case TokenKind::BinaryLiteral:
				if (IsBinary(stream->Peek())) {
					stream->AdvanceColumnsWhile(IsBinary);
					break;
				} else {
					tokenizer.PushError("Expect binary literal, but got '%c'", stream->Peek());
					return(result);
				}

			default:
			{
				tokenizer.PushError("Unexpected token kind '%s' for integer literal", GetTokenKindName(result.kind));
				return(result);
			}
		}
		bool hadIntegerLiteral = stream->textPos.index > s;
		if (result.kind != TokenKind::IntegerFloatLiteral && result.kind != TokenKind::HexadecimalFloatLiteral) {
			// @NOTE(final): Single quotes (') are allowed as separators for any non-decimal literal
			char check0 = stream->Peek();
			if (check0 == '\'') {
				if (!hadIntegerLiteral) {
					tokenizer.PushError("Too many single quote escape in integer literal, expect any integer literal but got '%c'", stream->Peek());
					return(result);
				}
				stream->AdvanceColumn();
				readNextLiteral = true;
			}
		}
	} while (!stream->IsEOF() && readNextLiteral);

	// Validate any literal after starting dot
	if (dotSeen) {
		if (firstLiteralPos == stream->textPos.index) {
			tokenizer.PushError("Expect any integer literal after starting dot, but got '%c'", stream->Peek());
			return(result);
		}
	}

	// Dot separator
	if ((!dotSeen) &&
		((result.kind == TokenKind::IntegerLiteral) ||
		(result.kind == TokenKind::HexLiteral) ||
			(result.kind == TokenKind::OctalLiteral)
			)) {
		char check0 = stream->Peek();
		if (check0 == '.') {
			dotSeen = true;
			stream->AdvanceColumn();
			if (result.kind == TokenKind::IntegerLiteral || result.kind == TokenKind::OctalLiteral) {
				result.kind = TokenKind::IntegerFloatLiteral;
			} else {
				assert(result.kind == TokenKind::HexLiteral);
				result.kind = TokenKind::HexadecimalFloatLiteral;
			}
		} else if (IsExponentPrefix(check0)) {
			if (result.kind == TokenKind::IntegerLiteral || result.kind == TokenKind::OctalLiteral) {
				result.kind = TokenKind::IntegerFloatLiteral;
			} else {
				assert(result.kind == TokenKind::HexLiteral);
				result.kind = TokenKind::HexadecimalFloatLiteral;
			}
		}
	}

	// Decimal after dot separator
	if ((result.kind != TokenKind::IntegerFloatLiteral) &&
		(result.kind != TokenKind::HexadecimalFloatLiteral)) {
		// Integer suffix
		if (IsIntegerSuffix(stream->Peek())) {
			stream->AdvanceColumnsWhile(IsIntegerSuffix, 3);
		}
	} else {
		if (result.kind == TokenKind::IntegerFloatLiteral) {
			// Float decimal
			if (IsNumeric(stream->Peek())) {
				stream->AdvanceColumnsWhile(IsNumeric);
			}
			if (stream->Peek() == 'e' || stream->Peek() == 'E') {
				if (!LexExponent(tokenizer)) {
					assert(tokenizer.errors.count > 0);
					return(result);
				}
			}
		} else {
			// Hex decimal
			assert(result.kind == TokenKind::HexadecimalFloatLiteral);
			if (IsHex(stream->Peek())) {
				stream->AdvanceColumnsWhile(IsHex);
			}
			if (stream->Peek() == 'p' || stream->Peek() == 'P') {
				if (!LexExponent(tokenizer)) {
					assert(tokenizer.errors.count > 0);
					return(result);
				}
			}
		}

		// Float suffix
		if (IsFloatSuffix(stream->Peek())) {
			stream->AdvanceColumn();
		}
	}

	return(result);
}

static TokenResult LexStringOrCharLiteral(Tokenizer &tokenizer, TokenKind kind, char quoteChar, size_t minCount, size_t maxCount) {
	BufferedStream *stream = tokenizer.stream;
	assert(stream->Peek() == quoteChar);
	stream->AdvanceColumn();
	TokenResult result = {};
	result.kind = kind;
	size_t count = 0;
	while (!stream->IsEOF()) {
		char first = stream->Peek();
		char second = stream->Peek(1);
		if (first == quoteChar) {
			result.isComplete = true;
			stream->AdvanceColumn();
			break;
		} else if (first == '\\') {
			switch (second) {
				case '\'':
				case '"':
				case '?':
				case '\\':
				case 'a':
				case 'b':
				case 'f':
				case 'n':
				case 'e':
				case 'r':
				case 't':
				case 'v':
				{
					stream->AdvanceColumn(2); // Skip over backslash and char
					++count;
					continue;
				}

				case 'x':
				case 'X':
				case 'u':
				case 'U':
				{
					stream->AdvanceColumn(2); // Skip over backslash and char
					if (IsHex(stream->Peek())) {
						int len = 0;
						while (!stream->IsEOF()) {
							if (!IsHex(stream->Peek()))
								break;
							++len;
							stream->AdvanceColumn();
						}
					} else {
						tokenizer.PushError("Unsupported hex escape character '{Buffer.Peek()}'!");
						break;
					}
					++count;
					continue;
				}

				default:
					stream->AdvanceColumn(); // Skip over backslash
					if (IsOctal(stream->Peek())) {
						while (!stream->IsEOF()) {
							if (!IsOctal(stream->Peek()))
								break;
							stream->AdvanceColumn();
						}
						++count;
						continue;
					} else {
						tokenizer.PushError("Not supported escape character '%c'", stream->Peek());
						break;
					}
			}
		} else if (IsLineBreak(first)) {
			tokenizer.PushError("Unexpected linebreak '%c'", first);
			break;
		} else if (IsTab(first)) {
			stream->AdvanceTab();
		} else {
			stream->AdvanceColumn();
		}
		++count;
	}
	if (!result.isComplete) {
		tokenizer.PushError("Unterminated string literal");
	} else {
		if (minCount > 0 && count < minCount)
			tokenizer.PushError("Not enough characters for {name} literal, expect {minCount} but got {count}!");
		else if (maxCount > 0 && (count > maxCount))
			tokenizer.PushError("Too many characters for {name} literal, expect {maxCount} but got {count}!");
	}
	return(result);
}

static TokenResult LexStringLiteral(Tokenizer &tokenizer) {
	TokenResult result = LexStringOrCharLiteral(tokenizer, TokenKind::StringLiteral, '"', 0, 0);
	return(result);
}

static TokenResult LexCharLiteral(Tokenizer &tokenizer) {
	TokenResult result = LexStringOrCharLiteral(tokenizer, TokenKind::CharLiteral, '\'', 1, 1);
	return(result);
}

static Tokenizer TokenizeCpp(BufferedStream *stream, MemoryBlock *memory) {
	Tokenizer result = {};
	result.memory = memory;
	result.stream = stream;
	result.tokens.minCapacity = 1024;
	result.errors.minCapacity = 128;
	while (!stream->IsEOF()) {
		stream->SkipWhitespaces();
		result.StartLexeme();
		size_t oldPos = stream->textPos.index;
		char first = stream->Peek();
		char second = stream->Peek(1);
		char third = stream->Peek(2);
		TokenResult currentResult = {};
		currentResult.isComplete = true;
		switch (first) {
			case '/':
			{
				if (second == '*') {
					currentResult = LexMultiLineComment(result);
				} else if (second == '/') {
					currentResult = LexSingleLineComment(result);
				} else if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::DivAssign;
				} else {
					currentResult.kind = TokenKind::DivOp;
				}
			} break;

			case '*':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::MulAssign;
				} else {
					currentResult.kind = TokenKind::MulOp;
				}
			} break;

			case '+':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::AddAssign;
				} else if (second == '+') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::IncOp;
				} else {
					currentResult.kind = TokenKind::AddOp;
				}
			} break;

			case '-':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::SubAssign;
				} else if (second == '-') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::DecOp;
				} else if (second == '>') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::PtrOp;
				} else {
					currentResult.kind = TokenKind::SubOp;
				}
			} break;

			case '<':
			{
				if (second == '<') {
					if (third == '=') {
						stream->AdvanceColumn(3);
						currentResult.kind = TokenKind::LeftShiftAssign;
					} else {
						stream->AdvanceColumn(2);
						currentResult.kind = TokenKind::LeftShiftOp;
					}
				} else if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::LessOrEqualOp;
				} else {
					currentResult.kind = TokenKind::LessThanOp;
				}
			} break;

			case '>':
			{
				if (second == '>') {
					if (third == '=') {
						stream->AdvanceColumn(3);
						currentResult.kind = TokenKind::RightShiftAssign;
					} else {
						stream->AdvanceColumn(2);
						currentResult.kind = TokenKind::RightShiftOp;
					}
				} else if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::GreaterOrEqualOp;
				} else {
					currentResult.kind = TokenKind::GreaterThanOp;
				}
			} break;

			case '%':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::ModAssign;
				} else {
					currentResult.kind = TokenKind::ModOp;
				}
			} break;

			case '=':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::LogicalEqualsOp;
				} else {
					currentResult.kind = TokenKind::EqOp;
				}
			} break;

			case '!':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::LogicalNotEqualsOp;
				} else {
					currentResult.kind = TokenKind::ExclationMark;
				}
			} break;

			case '.':
			{
				if (second == '.' && third == '.') {
					stream->AdvanceColumn(3);
					currentResult.kind = TokenKind::Ellipsis;
				} else if (IsNumeric(second)) {
					currentResult = LexNumber(result);
				} else {
					currentResult.kind = TokenKind::Dot;
				}
			} break;

			case '&':
			{
				if (second == '&') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::LogicalAndOp;
				} else if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::AndAssign;
				} else {
					currentResult.kind = TokenKind::AndOp;
				}
			} break;

			case '|':
			{
				if (second == '|') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::LogicalOrOp;
				} else if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::OrAssign;
				} else {
					currentResult.kind = TokenKind::OrOp;
				}
			} break;

			case '^':
			{
				if (second == '=') {
					stream->AdvanceColumn(2);
					currentResult.kind = TokenKind::XorAssign;
				} else {
					currentResult.kind = TokenKind::XorOp;
				}
			} break;

			case '\'':
				currentResult = LexCharLiteral(result);
				break;

			case '"':
				currentResult = LexStringLiteral(result);
				break;

			case '#':
				currentResult.kind = TokenKind::Raute;
				break;
			case '?':
				currentResult.kind = TokenKind::QuestionMark;
				break;
			case '~':
				currentResult.kind = TokenKind::Tilde;
				break;
			case ':':
				currentResult.kind = TokenKind::Colon;
				break;
			case ',':
				currentResult.kind = TokenKind::Comma;
				break;
			case ';':
				currentResult.kind = TokenKind::Semicolon;
				break;
			case '\\':
				currentResult.kind = TokenKind::Backslash;
				break;
			case '(':
				currentResult.kind = TokenKind::LeftParen;
				break;
			case ')':
				currentResult.kind = TokenKind::RightParen;
				break;
			case '{':
				currentResult.kind = TokenKind::LeftBrace;
				break;
			case '}':
				currentResult.kind = TokenKind::RightBrace;
				break;
			case '[':
				currentResult.kind = TokenKind::LeftBracket;
				break;
			case ']':
				currentResult.kind = TokenKind::RightBracket;
				break;

			default:
			{
				if (IsIdentStart(first)) {
					currentResult = LexIdent(result);
				} else if (IsNumeric(first)) {
					currentResult = LexNumber(result);
				} else if (IsLineBreak(first)) {
					currentResult.kind = TokenKind::EndOfLine;
					currentResult.isComplete = true;
					size_t nb = GetLineBreaks(first, second);
					stream->AdvanceLine(nb);
				} else if (IsTab(first)) {
					currentResult.kind = TokenKind::Spacings;
					currentResult.isComplete = true;
					while (!stream->IsEOF()) {
						if (stream->Peek() == '\t')
							stream->AdvanceTab();
						else
							break;
					}
				} else if (IsSpacing(first)) {
					currentResult.kind = TokenKind::Spacings;
					currentResult.isComplete = true;
					stream->AdvanceColumnsWhile(IsSpacing);
				} else {
					fprintf_s(stderr, "Unsupported character '%c' on line: %zu, col: %zu\n", first, stream->textPos.line + 1, stream->textPos.column + 1);
				}
			} break;
		}
		// Advance when position has not been changed
		if (oldPos == stream->textPos.index) {
			stream->AdvanceColumn();
		}
		if (currentResult.kind != TokenKind::Unknown) {
			result.PushLexeme(currentResult.kind, currentResult.isComplete);
		}
		assert(oldPos != stream->textPos.index);
	}
	return(result);
}

#define WIN32_FUNC_SHGetSpecialFolderPathA(name) BOOL WINAPI name(HWND hwnd, LPSTR pszPath, int csidl, BOOL fCreate)
typedef WIN32_FUNC_SHGetSpecialFolderPathA(win32_func_SHGetSpecialFolderPathA);

struct TokenMatch {
	TokenKind expectedKind;
	size_t expectedLength;
};

static bool TestTokenizerPart(const char *source, const TokenMatch *matches, size_t matchCount) {
	MemoryBlock memory = {};
	BufferedStream stream = {};
	stream.text.data = source;
	stream.text.length = strlen(source);
	Tokenizer tokenizer = TokenizeCpp(&stream, &memory);
	bool result = false;
	if (tokenizer.errors.count == 0) {
		if (tokenizer.tokens.count >= matchCount) {
			int err = 0;
			for (size_t i = 0; i < matchCount; ++i) {
				const TokenMatch *match = matches + i;
				const Token *token = tokenizer.tokens.items + 0;
				if (token->kind != match->expectedKind) {
					fprintf(stderr, "[Test] Expect token-%zu kind '%s', but got '%s' for source '%s'\n", (i + 1ULL), GetTokenKindName(match->expectedKind), GetTokenKindName(token->kind), source);
					++err;
				} else if (token->length != match->expectedLength) {
					fprintf(stderr, "[Test] Expect token-%zu length of '%zu', but got '%zu' for source '%s'\n", (i + 1ULL), match->expectedLength, token->length, source);
					++err;
				}
			}
			if (err == 0) {
				result = true;
			}
		} else if (tokenizer.tokens.count == 0) {
			// No tokens found
			fprintf(stderr, "[Test] Expect at least %zu tokens, but found %zu for source '%s'\n", matchCount, tokenizer.tokens.count, source);
		}
	} else {
		fprintf(stderr, "[Test] Error: (%s) for source '%s'\n", tokenizer.errors.items[0].message, source);
	}
	memory.Release();
	return(result);
}

static bool TestTokenizerPart(const char *source, TokenKind expectedKind, size_t expectedLength) {
	TokenMatch m[] = {
		{ expectedKind, expectedLength },
	};
	bool result = TestTokenizerPart(source, m, ArrayCount(m));
	return(result);
}

static bool TestTokenizerPart(const char *source, TokenMatch match1) {
	TokenMatch m[] = {
		match1,
	};
	bool result = TestTokenizerPart(source, m, ArrayCount(m));
	return(result);
}
static bool TestTokenizerPart(const char *source, TokenMatch match1, TokenMatch match2) {
	TokenMatch m[] = {
		match1,
		match2,
	};
	bool result = TestTokenizerPart(source, m, ArrayCount(m));
	return(result);
}

static bool TestTokenizerPart(const char *source, TokenMatch match1, TokenMatch match2, TokenMatch match3) {
	TokenMatch m[] = {
		match1,
		match2,
		match3,
	};
	bool result = TestTokenizerPart(source, m, ArrayCount(m));
	return(result);
}

static void TestTokenizer() {
	assert(TestTokenizerPart("", nullptr, 0));

	assert(TestTokenizerPart("0", TokenKind::OctalLiteral, 1));
	assert(TestTokenizerPart("42", TokenKind::IntegerLiteral, 2));
	assert(TestTokenizerPart("052", TokenKind::OctalLiteral, 3));
	assert(TestTokenizerPart("0x2a", TokenKind::HexLiteral, 4));
	assert(TestTokenizerPart("0X2A", TokenKind::HexLiteral, 4));
	assert(TestTokenizerPart("0b101010", TokenKind::BinaryLiteral, 8));

	assert(TestTokenizerPart("123", TokenKind::IntegerLiteral, 3));
	assert(TestTokenizerPart("0x123", TokenKind::HexLiteral, 5));
	assert(TestTokenizerPart("0b10", TokenKind::BinaryLiteral, 4));
	assert(TestTokenizerPart("12345678901234567890ull", TokenKind::IntegerLiteral, 23));
	assert(TestTokenizerPart("12345678901234567890u", TokenKind::IntegerLiteral, 21));

	assert(TestTokenizerPart("18446744073709550592ull", TokenKind::IntegerLiteral, 23));
	assert(TestTokenizerPart("18'446'744'073'709'550'592llu", TokenKind::IntegerLiteral, 29));
	assert(TestTokenizerPart("1844'6744'0737'0955'0592uLL", TokenKind::IntegerLiteral, 27));
	assert(TestTokenizerPart("184467'440737'0'95505'92LLU", TokenKind::IntegerLiteral, 27));

	// We dont support hex numbers ending with E or starting an exponent without the E prefix
	assert(!TestTokenizerPart("0xE+2.0", TokenKind::Unknown, 0));
	assert(!TestTokenizerPart("0xa+2.0", TokenKind::HexadecimalFloatLiteral, 7));
	assert(!TestTokenizerPart("0xE +2.0", TokenKind::HexadecimalFloatLiteral, 8));
	assert(!TestTokenizerPart("(0xE)+2.0", TokenKind::HexadecimalFloatLiteral, 9));

	assert(TestTokenizerPart("1e10", TokenKind::IntegerFloatLiteral, 4));
	assert(TestTokenizerPart("1e-5L", TokenKind::IntegerFloatLiteral, 5));
	assert(TestTokenizerPart("1.", TokenKind::IntegerFloatLiteral, 2));
	assert(TestTokenizerPart(".5", TokenKind::IntegerFloatLiteral, 2));
	assert(TestTokenizerPart(".1f", TokenKind::IntegerFloatLiteral, 3));
	assert(TestTokenizerPart("1.e-2", TokenKind::IntegerFloatLiteral, 5));
	assert(TestTokenizerPart("3.14", TokenKind::IntegerFloatLiteral, 4));
	assert(TestTokenizerPart("0.1e-1L", TokenKind::IntegerFloatLiteral, 7));

	assert(TestTokenizerPart("0x1ffp10", TokenKind::HexadecimalFloatLiteral, 8));
	assert(TestTokenizerPart("0X0p-1", TokenKind::HexadecimalFloatLiteral, 6));
	assert(TestTokenizerPart("0x1.p0", TokenKind::HexadecimalFloatLiteral, 6));
	assert(TestTokenizerPart("0xf.p-1", TokenKind::HexadecimalFloatLiteral, 7));
}

int main(int argc, char *argv[]) {
	TestTokenizer();

	if (argc < 2) {
		fprintf(stderr, "Missing filepath argument!\n");
		return(-1);
	}

	const char *filePath = argv[1];

	FILE *inFile = nullptr;
	if (fopen_s(&inFile, filePath, "rb") != 0) {
		fprintf(stderr, "File '%s' does not exists!\n", filePath);
		return(-1);
	}

	Text text = {};
	fseek(inFile, 0, SEEK_END);
	size_t textLen = ftell(inFile);

	MemoryBlock memory = {};
#if !defined(NO_MEMORY_BLOCKS)
	memory.Allocate(512 * 1024 * 1024); // Start with a 512 MB sized block
#endif

	for (size_t i = 0, c = ArrayCount(gGlobalClassKeywords); i < c; ++i) {
		const char *keyword = gGlobalClassKeywords[i];
		globalClassKeywordsHashTable.Add(&memory, keyword, strlen(keyword));
	}
	for (size_t i = 0, c = ArrayCount(gTypeKeywords); i < c; ++i) {
		const char *keyword = gTypeKeywords[i];
		globalTypeKeywordsHashTable.Add(&memory, keyword, strlen(keyword));
	}
	for (size_t i = 0, c = ArrayCount(gReservedKeywords); i < c; ++i) {
		const char *keyword = gReservedKeywords[i];
		globalReservedKeywordsHashTable.Add(&memory, keyword, strlen(keyword));
	}

	size_t textDataSize = sizeof(char) * (textLen + 1);
	char *textData = memory.PushSize<char>(textDataSize);
	if (textData == nullptr) {
		fclose(inFile);
		fprintf(stderr, "Failed to allocate %zu bytes of memory for text data\n", textDataSize);
		return(-1);
	}

	fseek(inFile, 0, SEEK_SET);
	size_t s = fread_s(textData, textLen, 1, textLen, inFile);
	fclose(inFile);
	if (s != textLen) {
		fprintf(stderr, "Failed to %zu bytes from file '%s'\n", textDataSize, filePath);
		return(-1);
	}
	textData[textLen] = 0;
	text.data = (const char *)textData;
	text.length = textLen;

	BufferedStream bufferedStream = {};
	bufferedStream.text = text;
	bufferedStream.columnsPerTab = 4;

	Stopwatch watch = Stopwatch::StartNow();
	Tokenizer tokenizer = TokenizeCpp(&bufferedStream, &memory);
	watch.Stop();
	printf("Tokenize cpp done, %zu tokens, took %.6f ms\n", tokenizer.tokens.count, watch.elapsed.totalMilliseconds);

	HMODULE shellLib = LoadLibraryA("shell32.dll");
	win32_func_SHGetSpecialFolderPathA *SHGetSpecialFolderPathA = (win32_func_SHGetSpecialFolderPathA *)GetProcAddress(shellLib, "SHGetSpecialFolderPathA");

	char desktopPath[MAX_PATH + 1];
	SHGetSpecialFolderPathA(NULL, desktopPath, CSIDL_DESKTOP, FALSE);

	const char *outFilename = "\\tokenizer_raw_c.txt";
	char outFilePath[MAX_PATH + 1];
	strcpy_s(outFilePath, sizeof(outFilePath), desktopPath);
	strncat_s(outFilePath, sizeof(outFilePath), outFilename, strlen(outFilename));

	FILE *outFile = nullptr;
	if (fopen_s(&outFile, outFilePath, "wb") == 0) {
		for (size_t i = 0; i < tokenizer.tokens.count; ++i) {
			const Token *token = tokenizer.tokens.items + i;
			const char *tokenKindName = GetTokenKindName(token->kind);
			fprintf(outFile, "%s@%zu (line: %zu, column: %zu, length: %zu)\n", tokenKindName, token->position.index, token->position.line, token->position.column, token->length);
		}
		fflush(outFile);
		fclose(outFile);
	} else {
		fprintf(stderr, "Failed to open file '%s' for writing!\n", outFilePath);
	}

	memory.Release();

	printf("Press any key to exit\n");
	getchar();
	return(0);
}