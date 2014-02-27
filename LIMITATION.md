# Below listed parts of HOCON spec are not currently implemented

This is an informal spec, but hopefully it's clear.

### Unchanged from JSON

 - values have possible types: string, number, object, array, boolean, **null**

### Commas

The last element in an array or last field in an object may be
followed by a single comma. This extra comma is ignored.

 - `[1,2,3,,]` is invalid because it has two trailing commas.
 - `[,1,2,3]` is invalid because it has an initial comma.
 - `[1,,2,3]` is invalid because it has two commas in a row.
 - these same comma rules apply to fields in objects.

### Whitespace

The JSON spec simply says "whitespace"; in HOCON whitespace is
defined as follows:

 - any Unicode space separator (Zs category), line separator (Zl
   category), or paragraph separator (Zp category), including
   nonbreaking spaces (such as 0x00A0, 0x2007, and 0x202F).
   The BOM (0xFEFF) must also be treated as whitespace.
 - tab (`\t` 0x0009), newline ('\n' 0x000A), vertical tab ('\v'
   0x000B)`, form feed (`\f' 0x000C), carriage return ('\r'
   0x000D), file separator (0x001C), group separator (0x001D),
   record separator (0x001E), unit separator (0x001F).

While all Unicode separators should be treated as whitespace, in
this spec "newline" refers only and specifically to ASCII newline
0x000A.

### Duplicate keys and object merging

Object merge can be prevented by setting the key to another value
first. This is because merging is always done two values at a
time; if you set a key to an object, a non-object, then an object,
first the non-object falls back to the object (non-object always
wins), and then the object falls back to the non-object (no
merging, object is the new value). So the two objects never see
each other.

And these two are equivalent:

    {
        "foo" : { "a" : 42 },
        "foo" : null,
        "foo" : { "b" : 43 }
    }

    {
        "foo" : { "b" : 43 }
    }

The intermediate setting of `"foo"` to `null` prevents the object merge.

### Unquoted strings

A sequence of characters outside of a quoted string is a string
value if:

 - it does not contain "forbidden characters": '$', '"', '{', '}',
   '[', ']', ':', '=', ',', '+', '#', '`', '^', '?', '!', '@',
   '*', '&', '\' (backslash), or whitespace.
 - it does not contain the two-character string "//" (which
   starts a comment)
 - its initial characters do not parse as `true`, `false`, `null`,
   or a number.

Unquoted strings are used literally, they do not support any kind
of escaping. Quoted strings may always be used as an alternative
when you need to write a character that is not permitted in an
unquoted string.

`truefoo` parses as the boolean token `true` followed by the
unquoted string `foo`. However, `footrue` parses as the unquoted
string `footrue`. Similarly, `10.0bar` is the number `10.0` then
the unquoted string `bar` but `bar10.0` is the unquoted string
`bar10.0`. (In practice, this distinction doesn't matter much
because of value concatenation; see later section.)

In general, once an unquoted string begins, it continues until a
forbidden character or the two-character string "//" is
encountered. Embedded (non-initial) booleans, nulls, and numbers
are not recognized as such, they are part of the string.

An unquoted string may not _begin_ with the digits 0-9 or with a
hyphen (`-`, 0x002D) because those are valid characters to begin a
JSON number. The initial number character, plus any valid-in-JSON
number characters that follow it, must be parsed as a number
value. Again, these characters are not special _inside_ an
unquoted string; they only trigger number parsing if they appear
initially.

Note that quoted JSON strings may not contain control characters
(control characters include some whitespace characters, such as
newline). This rule is from the JSON spec. However, unquoted
strings have no restriction on control characters, other than the
ones listed as "forbidden characters" above.

Some of the "forbidden characters" are forbidden because they
already have meaning in JSON or HOCON, others are essentially
reserved keywords to allow future extensions to this spec.

### Multi-line strings

Multi-line strings are similar to Python or Scala, using triple
quotes. If the three-character sequence `"""` appears, then all
Unicode characters until a closing `"""` sequence are used
unmodified to create a string value. Newlines and whitespace
receive no special treatment. Unlike Scala, and unlike JSON quoted
strings, Unicode escapes are not interpreted in triple-quoted
strings.

In Python, `"""foo""""` is a syntax error (a triple-quoted string
followed by a dangling unbalanced quote). In Scala, it is a
four-character string `foo"`. HOCON works like Scala; any sequence
of at least three quotes ends the multi-line string, and any
"extra" quotes are part of the string.

### Value concatenation

The value of an object field or array element may consist of
multiple values which are combined. There are three kinds of value
concatenation:

 - if all the values are simple values (neither objects nor
   arrays), they are concatenated into a string.
 - if all the values are arrays, they are concatenated into
   one array.
 - if all the values are objects, they are merged (as with
   duplicate keys) into one object.

String value concatenation is allowed in field keys, in addition
to field values and array elements. Objects and arrays do not make
sense as field keys.

#### String value concatenation

String value concatenation is the trick that makes unquoted
strings work; it also supports substitutions (`${foo}` syntax) in
strings.

Only simple values participate in string value
concatenation. Recall that a simple value is any value other than
arrays and objects.

As long as simple values are separated only by non-newline
whitespace, the _whitespace between them is preserved_ and the
values, along with the whitespace, are concatenated into a string.

String value concatenations never span a newline, or a character
that is not part of a simple value.

A string value concatenation may appear in any place that a string
may appear, including object keys, object values, and array
elements.

Whenever a value would appear in JSON, a HOCON parser instead
collects multiple values (including the whitespace between them)
and concatenates those values into a string.

Whitespace before the first and after the last simple value must
be discarded. Only whitespace _between_ simple values must be
preserved.

So for example ` foo bar baz ` parses as three unquoted strings,
and the three are value-concatenated into one string. The inner
whitespace is kept and the leading and trailing whitespace is
trimmed. The equivalent string, written in quoted form, would be
`"foo bar baz"`.

Value concatenating `foo bar` (two unquoted strings with
whitespace) and quoted string `"foo bar"` would result in the same
in-memory representation, seven characters.

For purposes of string value concatenation, non-string values are
converted to strings as follows (strings shown as quoted strings):

 - `true` and `false` become the strings `"true"` and `"false"`.
 - `null` becomes the string `"null"`.
 - quoted and unquoted strings are themselves.
 - numbers should be kept as they were originally written in the
   file. For example, if you parse `1e5` then you might render
   it alternatively as `1E5` with capital `E`, or just `100000`.
   For purposes of value concatenation, it should be rendered
   as it was written in the file.
 - a substitution is replaced with its value which is then
   converted to a string as above.
 - it is invalid for arrays or objects to appear in a string value
   concatenation.

A single value is never converted to a string. That is, it would
be wrong to value concatenate `true` by itself; that should be
parsed as a boolean-typed value. Only `true foo` (`true` with
another simple value on the same line) should be parsed as a value
concatenation and converted to a string.

#### Array and object concatenation

Arrays can be concatenated with arrays, and objects with objects,
but it is an error if they are mixed.

For purposes of concatenation, "array" also means "substitution
that resolves to an array" and "object" also means "substitution
that resolves to an object."

Within an field value or array element, if only non-newline
whitespace separates the end of a first array or object or
substitution from the start of a second array or object or
substitution, the two values are concatenated. Newlines may occur
_within_ the array or object, but not _between_ them. Newlines
_between_ prevent concatenation.

For objects, "concatenation" means "merging", so the second object
overrides the first.

Arrays and objects cannot be field keys, whether concatenation is
involved or not.

Here are several ways to define `a` to the same object value:

    // one object
    a : { b : 1, c : 2 }
    // two objects that are merged via concatenation rules
    a : { b : 1 } { c : 2 }
    // two fields that are merged
    a : { b : 1 }
    a : { c : 2 }

Here are several ways to define `a` to the same array value:

    // one array
    a : [ 1, 2, 3, 4 ]
    // two arrays that are concatenated
    a : [ 1, 2 ] [ 3, 4 ]
    // a later definition referring to an earlier
    // (see "self-referential substitutions" below)
    a : [ 1, 2 ]
    a : ${a} [ 3, 4 ]

A common use of array concatenation is to add to paths:

    path = [ /bin ]
    path = ${path} [ /usr/bin ]

#### Note: Arrays without commas or newlines

Arrays allow you to use newlines instead of commas, but not
whitespace instead of commas. Non-newline whitespace will produce
concatenation rather than separate elements.

    // this is an array with one element, the string "1 2 3 4"
    [ 1 2 3 4 ]
    // this is an array of four integers
    [ 1
      2
      3
      4 ]

    // an array of one element, the array [ 1, 2, 3, 4 ]
    [ [ 1, 2 ] [ 3, 4 ] ]
    // an array of two arrays
    [ [ 1, 2 ]
      [ 3, 4 ] ]

If this gets confusing, just use commas. The concatenation
behavior is useful rather than surprising in cases like:

    [ This is an unquoted string my name is ${name}, Hello ${world} ]
    [ ${a} ${b}, ${x} ${y} ]

Non-newline whitespace is never an element or field separator.

### Paths as keys

If a key is a path expression with multiple elements, it is
expanded to create an object for each path element other than the
last. The last path element, combined with the value, becomes a
field in the most-nested object.

These values are merged in the usual way; which implies
that:

    a.x : 42, a.y : 43

is equivalent to:

    a { x : 42, y : 43 }

Because path expressions work like value concatenations, you can
have whitespace in keys:

    a b c : 42

is equivalent to:

    "a b c" : 42

Because path expressions are always converted to strings, even
single values that would normally have another type become
strings.

   - `true : 42` is `"true" : 42`
   - `3 : 42` is `"3" : 42`
   - `3.14 : 42` is `"3" : { "14" : 42 }`

As a special rule, the unquoted string `include` may not begin a
path expression in a key, because it has a special interpretation
(see below).

### Substitutions

Substitutions are not parsed inside quoted strings. To get a
string containing a substitution, you must use value concatenation
with the substitution in the unquoted portion:

    key : ${animal.favorite} is my favorite animal

Or you could quote the non-substitution portion:

    key : ${animal.favorite}" is my favorite animal"

Substitutions are resolved by looking up the path in the
configuration. The path begins with the root configuration object,
i.e. it is "absolute" rather than "relative."

Substitution processing is performed as the last parsing step, so
a substitution can look forward in the configuration. If a
configuration consists of multiple files, it may even end up
retrieving a value from another file.

If a key has been specified more than once, the substitution will
always evaluate to its latest-assigned value (that is, it will
evaluate to the merged object, or the last non-object value that
was set, in the entire document being parsed including all
included files).

If a configuration sets a value to `null` then it should not be
looked up in the external source. Unfortunately there is no way to
"undo" this in a later configuration file; if you have `{ "HOME" :
null }` in a root object, then `${HOME}` will never look at the
environment variable. There is no equivalent to JavaScript's
`delete` operation in other words.

If a substitution does not match any value present in the
configuration and is not resolved by an external source, then it
is undefined. An undefined substitution with the `${foo}` syntax
is invalid and should generate an error.

#### Self-Referential Substitutions
Examples of self-referential fields:

 - `a : ${a}bc`
 - `path : ${path} [ /usr/bin ]`

#### The `+=` field separator

Fields may have `+=` as a separator rather than `:` or `=`. A
field with `+=` transforms into a self-referential array
concatenation, like this:

    a += b

becomes:

    a = ${?a} [b]

`+=` appends an element to a previous array. If the previous value
was not an array, an error will result just as it would in the
long form `a = ${?a} [b]`. Note that the previous value is
optional (`${?a}` not `${a}`), which allows `a += b` to be the
first mention of `a` in the file (it is not necessary to have `a =
[]` first).

#### Examples of Self-Referential Substitutions

In isolation (with no merges involved), a self-referential field
is an error because the substitution cannot be resolved:

    foo : ${foo} // an error

When `foo : ${foo}` is merged with an earlier value for `foo`,
however, the substitution can be resolved to that earlier value.
When merging two objects, the self-reference in the overriding
field refers to the overridden field. Say you have:

    foo : { a : 1 }

and then:

    foo : ${foo}

Then `${foo}` resolves to `{ a : 1 }`, the value of the overridden
field.

It would be an error if these two fields were reversed, so first:

    foo : ${foo}

and then second:

    foo : { a : 1 }

Here the `${foo}` self-reference comes before `foo` has a value,
so it is undefined, exactly as if the substitution referenced a
path not found in the document.

Because `foo : ${foo}` conceptually looks to previous definitions
of `foo` for a value, the error should be treated as "undefined"
rather than "intractable cycle"; as a result, the optional
substitution syntax `${?foo}` does not create a cycle:

    foo : ${?foo} // this field just disappears silently

If a substitution is hidden by a value that could not be merged
with it (by a non-object value) then it is never evaluated and no
error will be reported. So for example:

    foo : ${does-not-exist}
    foo : 42

In this case, no matter what `${does-not-exist}` resolves to, we
know `foo` is `42`, so `${does-not-exist}` is never evaluated and
there is no error. The same is true for cycles like `foo : ${foo},
foo : 42`, where the initial self-reference must simply be ignored.

A self-reference resolves to the value "below" even if it's part
of a path expression. So for example:

    foo : { a : { c : 1 } }
    foo : ${foo.a}
    foo : { a : 2 }

Another tricky case is an optional self-reference in a value
concatenation, in this example `a` should be `foo` not `foofoo`
because the self reference has to "look back" to an undefined `a`:

    a = ${?a}foo

### Includes

#### Include syntax

It may appear later in the key:

    # this is valid
    { foo include : 42 }
    # equivalent to
    { "foo include" : 42 }

Say for example that the root configuration is this:

    { a : { include "foo.conf" } }

Say that the root configuration redefines `a.x`, like this:

    {
        a : { include "foo.conf" }
        a : { x : 42 }
    }

### Conversion of numerically-indexed objects to arrays

In some file formats and contexts, such as Java properties files,
there isn't a good way to define arrays. To provide some mechanism
for this, implementations should support converting objects with
numeric keys into arrays. For example, this object:

    { "0" : "a", "1" : "b" }

could be treated as:

    [ "a", "b" ]

This allows creating an array in a properties file like this:

    foo.0 = "a"
    foo.1 = "b"

The details:

 - the conversion should be done lazily when required to avoid
   a type error, NOT eagerly anytime an object has numeric
   keys.
 - the conversion should be done when you would do an automatic
   type conversion (see the section "Automatic type conversions"
   below).
 - the conversion should be done in a concatenation when a list
   is expected and an object with numeric keys is found.
 - the conversion should not occur if the object is empty or
   has no keys which parse as positive integers.
 - the conversion should ignore any keys which do not parse
   as positive integers.
 - the conversion should sort by the integer value of each
   key and then build the array; if the integer keys are "0" and
   "2" then the resulting array would have indices "0" and "1",
   i.e. missing indices in the object are eliminated.

### Automatic type conversions

If an application asks for a value with a particular type, the
implementation should attempt to convert types as follows:

 - number to string: convert the number into a string
   representation that would be a valid number in JSON.
 - boolean to string: should become the string "true" or "false"
 - string to number: parse the number with the JSON rules
 - string to boolean: the strings "true", "yes", "on", "false",
   "no", "off" should be converted to boolean values. It's
   tempting to support a long list of other ways to write a
   boolean, but for interoperability and keeping it simple, it's
   recommended to stick to these six.
 - string to null: the string `"null"` should be converted to a
   null value if the application specifically asks for a null
   value, though there's probably no reason an app would do this.
 - numerically-indexed object to array: see the section
   "Conversion of numerically-indexed objects to arrays" above

The following type conversions should NOT be performed:

 - null to anything: If the application asks for a specific type
   and finds null instead, that should usually result in an error.
 - object to anything
 - array to anything
 - anything to object
 - anything to array, with the exception of numerically-indexed
   object to array

Converting objects and arrays to and from strings is tempting, but
in practical situations raises thorny issues of quoting and
double-escaping.

### Units format

Implementations may wish to support interpreting a value with some
family of units, such as time units or memory size units: `10ms`
or `512K`. HOCON does not have an extensible type system and there
is no way to add a "duration" type. However, for example, if an
application asks for milliseconds, the implementation can try to
interpret a value as a milliseconds value.

If an API supports this, for each family of units it should define
a default unit in the family. For example, the family of duration
units might default to milliseconds (see below for details on
durations). The implementation should then interpret values as
follows:

 - if the value is a number, it is taken to be a number in
   the default unit.
 - if the value is a string, it is taken to be this sequence:

     - optional whitespace
     - a number
     - optional whitespace
     - an optional unit name consisting only of letters (letters
       are the Unicode `L*` categories, Java `isLetter()`)
     - optional whitespace

   If a string value has no unit name, then it should be
   interpreted with the default unit, as if it were a number. If a
   string value has a unit name, that name of course specifies the
   value's interpretation.

### Duration format

Implementations may wish to support a `getMilliseconds()` (and
similar for other time units).

This can use the general "units format" described above; bare
numbers are taken to be in milliseconds already, while strings are
parsed as a number plus an optional unit string.

The supported unit strings for duration are case sensitive and
must be lowercase. Exactly these strings are supported:

 - `ns`, `nanosecond`, `nanoseconds`
 - `us`, `microsecond`, `microseconds`
 - `ms`, `millisecond`, `milliseconds`
 - `s`, `second`, `seconds`
 - `m`, `minute`, `minutes`
 - `h`, `hour`, `hours`
 - `d`, `day`, `days`

### Size in bytes format

Implementations may wish to support a `getBytes()` returning a
size in bytes.

This can use the general "units format" described above; bare
numbers are taken to be in bytes already, while strings are
parsed as a number plus an optional unit string.

The one-letter unit strings may be uppercase (note: duration units
are always lowercase, so this convention is specific to size
units).

There is an unfortunate nightmare with size-in-bytes units, that
they may be in powers or two or powers of ten. The approach
defined by standards bodies appears to differ from common usage,
such that following the standard leads to people being confused.
Worse, common usage varies based on whether people are talking
about RAM or disk sizes, and various existing operating systems
and apps do all kinds of different things.  See
http://en.wikipedia.org/wiki/Binary_prefix#Deviation_between_powers_of_1024_and_powers_of_1000
for examples. It appears impossible to sort this out without
causing confusion for someone sometime.

For single bytes, exactly these strings are supported:

 - `B`, `b`, `byte`, `bytes`

For powers of ten, exactly these strings are supported:

 - `kB`, `kilobyte`, `kilobytes`
 - `MB`, `megabyte`, `megabytes`
 - `GB`, `gigabyte`, `gigabytes`
 - `TB`, `terabyte`, `terabytes`
 - `PB`, `petabyte`, `petabytes`
 - `EB`, `exabyte`, `exabytes`
 - `ZB`, `zettabyte`, `zettabytes`
 - `YB`, `yottabyte`, `yottabytes`

For powers of two, exactly these strings are supported:

 - `K`, `k`, `Ki`, `KiB`, `kibibyte`, `kibibytes`
 - `M`, `m`, `Mi`, `MiB`, `mebibyte`, `mebibytes`
 - `G`, `g`, `Gi`, `GiB`, `gibibyte`, `gibibytes`
 - `T`, `t`, `Ti`, `TiB`, `tebibyte`, `tebibytes`
 - `P`, `p`, `Pi`, `PiB`, `pebibyte`, `pebibytes`
 - `E`, `e`, `Ei`, `EiB`, `exbibyte`, `exbibytes`
 - `Z`, `z`, `Zi`, `ZiB`, `zebibyte`, `zebibytes`
 - `Y`, `y`, `Yi`, `YiB`, `yobibyte`, `yobibytes`

It's very unclear which units the single-character abbreviations
("128K") should go with; some precedents such as `java -Xmx 2G`
and the GNU tools such as `ls` map these to powers of two, so this
spec copies that. You can certainly find examples of mapping these
to powers of ten, though. If you don't like ambiguity, don't use
the single-letter abbreviations.

### Config object merging and file merging

It may be useful to offer a method to merge two objects. If such a
method is provided, it should work as if the two objects were
duplicate values for the same key in the same file. (See the
section earlier on duplicate key handling.)

As with duplicate keys, an intermediate non-object value "hides"
earlier object values. So say you merge three objects in this
order:

 - `{ a : { x : 1 } }`  (first priority)
 - `{ a : 42 }` (fallback)
 - `{ a : { y : 2 } }` (another fallback)

The result would be `{ a : { x : 1 } }`. The two objects are not
merged because they are not "adjacent"; the merging is done in
pairs, and when `42` is paired with `{ y : 2 }`, `42` simply wins
and loses all information about what it overrode.

But if you re-ordered like this:

 - `{ a : { x : 1 } }`  (first priority)
 - `{ a : { y : 2 } }` (fallback)
 - `{ a : 42 }` (another fallback)

Now the result would be `{ a : { x : 1, y : 2 } }` because the two
objects are adjacent.

This rule for merging objects loaded from different files is
_exactly_ the same behavior as for merging duplicate fields in the
same file. All merging works the same way.

Needless to say, normally it's well-defined whether a config
setting is supposed to be a number or an object. This kind of
weird pathology where the two are mixed should not be happening.

The one place where it matters, though, is that it allows you to
"clear" an object and start over by setting it to null and then
setting it back to a new object. So this behavior gives people a
way to get rid of default fallback values they don't want.
