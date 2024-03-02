# Double-Cell Words

### `forth`

These words are in `forth`.

##### `2drop`
( d -- )

Double drop

##### `2swap`
( d1 d2 -- d2 d1 )

Double swap

##### `2over`
( d1 d2 -- d1 d2 d1 )

Double over

##### `2dup`
( d1 -- d1 d1 )

Double dup

##### `2nip`
( d1 d2 -- d2 )

Double nip

##### `2tuck`
( d1 d2 -- d2 d1 d2 )

Double tuck

##### `2rot`
( d1 d2 d3 -- d2 d3 d1 )

Double rot

##### `4dup`
( d1 d2 -- d1 d2 d1 d2 )

Quadruple dup

##### `2r@`
( R: d1 -- d1 ) ( -- d1 )

Read two cells from the top of the return stack.

##### `d=`
( d1 d2 -- f )

Test for the equality of two double cells

##### `d<>`
( d1 d2 -- f )

Test for the inequality of two double cells

##### `du<`
( ud1 ud2 -- f )

Unsigned double less than

##### `du>`
( ud1 ud2 -- f )

Unsigned double greater than

##### `du>=`
( ud1 ud2 -- f )

Unsigned double greater than or equal

##### `du<=`
( ud1 ud2 -- f )

Unsigned double less than or equal

##### `d<`
( nd1 nd2 -- f )

Signed double less than

##### `d>`
( nd1 nd2 -- f )

Signed double greater than

##### `d>=`
( nd1 nd2 -- f )

Signed double greater than or equal

##### `d<=`
( nd1 nd2 -- f )

Signed double less than or equal

##### `d0=`
( d -- f )

Double equal to zero

##### `d0<>`
( d -- f )

Double not equal to zero

##### `d0<`
( nd -- f )

Double less than zero

##### `d0>`
( nd -- f )

Double greater than zero

##### `d0>=`
( nd -- f )

Double greater than or equal to zero

##### `d0<=`
( nd -- f )

Double less than or equal to zero

##### `dnegate`
( nd1 -- nd2 )

Negate a double cell

##### `dabs`
( nd -- ud )

Double absolute value

##### `dmin`
( nd1 nd2 -- nd1|nd2 )

Double minimum

##### `dmax`
( nd1 nd2 -- nd1|nd2 )

Double maximum

##### `d+`
( d1 d2 -- d3 )

Add two double cells

##### `d-`
( d1 d2 -- d3 )

Subtract two double cells
	
##### `um+`
( u1 u2 -- u3 carry )

Add with carry

##### `um*`
( u1 u2 -- ud )

Multiply two unsigned 32-bit values to get an unsigned 64-bit value

##### `m*`
( n1 n2 -- d )

Multiply two signed 32-bit values to get a signed 64-bit value

##### `ud*`
( ud1 ud2 -- ud3 )

Unsigned multiply 64 * 64 = 64

##### `d*`
( nd1 nd2 -- nd3 )

Signed multiply 64 * 64 = 64

##### `*/`
( n1 n2 n3 -- n4 )

Multiply signed n1 and n2 to get double cell intermediate value, then divide it by n3 to get a single cell result
	
##### `*/mod`
( n1 n2 n3 -- n4 n5 )

Multiply signed n1 and n2 to get double cell intermediate value, then divide it by n3 to get a single cell remainder n4 and quotient n5

##### `u*/`
( u1 u2 u3 -- u4 )

Multiply unsigned u1 and u2 to get double cell intermediate value, then divide it by u3 to get a single cell result
	
##### `u*/mod`
( u1 u2 u3 -- u4 u5 )

Multiply unsigned u1 and u2 to get double cell intermediate value, then divide it by u3 to get a single cell remainder u4 and quotient u5

##### `um/mod`
( ud u1 -- u2 u3 )

Divide unsigned ud by u1 and get a single cell remainder u2 and quotient u3

##### `m/mod`
( nd n1 -- n2 n3 )

Divide signed nd by n1 and get a single cell remainder n2 and quotient n3

##### `ud/mod`
( ud1 ud2 -- ud3 ud4 )

Divide unsigned ud1 by ud2 and get double cell remainder ud3 and quotient ud4

##### `d/mod`
( nd1 nd2 -- nd3 nd4 )

Divide signed nd1 by nd2 and get double cell remainder nd3 and quotient nd4

##### `ud/`
( ud1 ud2 -- ud3 )

Divide unsigned two double cells and get a double cell quotient

##### `d/`
( nd1 nd2 -- nd3 )

Divide signed two double cells and get a double cell quotient

##### `f*`
( d1 d2 -- d3 )

Multiply two s31.32 fixed-point numbers. Note that overflow is possible, where then the sign will be wrong.

##### `f/`
( d1 d2 -- d3 )

Divide two s31.32 fixed-point numbers. Note that overflow is possible, where then the sign will be wrong.

##### `udm*`
( d1 d2 -- dl dh )

Multiply two 64-bit double-cell numbers into a single 128-bit quadruple-cell number.

##### `fi**`
( f1 u -- f2 )

Exponentiation of an S31.32 fixed-point number by an unsigned integer

##### `fmod`
( f1 f2 -- f3 )

Compute the symmetric modulus of two S31.32 S31.32 fixed-point numbers.

##### `ceil`
( f -- n )

Get the ceiling of a fixed-point number as a single-cell number.

##### `floor`
( f -- n )

Get the floor of a fixed-point number as a single-cell number.

##### `round-half-up`
( f -- n )

Round a fixed-point number up to the nearest integer with half rounding up.

##### `round-half-down`
( f -- n )

Round a fixed-point number down to the nearest integer with half rounding down.

##### `round-half-zero`
( f -- n )

Round a fixed-point number to the nearest integer with half rounding towards zero.

##### `round-half-away-zero`
( f -- n )

Round a fixed-point number to the nearest integer with half rounding away from zero.

##### `round-half-even`
( f -- n )

Round a fixed-point number to the nearest integer with half rounding towards even.

##### `round-half-odd`
( f -- n )

Round a fixed-point number to the nearest integer with half rounding towards even.

##### `round-zero`
( f -- n )

Round a fixed-point number towards zero.

##### `round-away-zero`
( f -- n )

Round a fixed-point number away from zero.

##### `sqrt`
( f1 -- f2 )

Square root of an S31.32 fixed-point number

##### `expm1`
( f1 -- f2 )

Calculate (e^x)-1 where x is an S31.32 fixed-point number

##### `exp`
( f1 -- f2 )

Calculate e^x where x is an S31.32 fixed-point number

##### `lnp1`
( f1 -- f2 )

Calculate ln(x + 1) where x is an S31.32 fixed-point number

##### `ln`
( f1 -- f2 )

Calculate ln(x) where x is an S31.32 fixed-point number

##### `f**`
( fb fx -- fb^x )

Calculate b^x where b and x are S31.32 fixed-point numbers

##### `sin`
( f1 -- f2 )

Calculate sin(x) where x is an S31.32 fixed-point number

##### `cos`
( f1 -- f2 )

Calculate cos(x) where x is an S31.32 fixed-point number

##### `tan`
( f1 -- f2 )

Calculate tan(x) where x is an S31.32 fixed-point number

##### `asin`
( f1 -- f2 )

Calculate asin(x) where x is an S31.32 fixed-point number

##### `acos`
( f1 -- f2 )

Calculate acos(x) where x is an S31.32 fixed-point number

##### `atan`
( f1 -- f2 )

Calculate atan(x) where x is an S31.32 fixed-point number

##### `atan2`
( fy fx -- fangle )

Calculate the angle of any pair of x and y coordinates where they are all S31.32 fixed-point numbers

##### `sinh`
( f1 -- f2 )

Calculate sinh(x) where x is an S31.32 fixed-point number

##### `cosh`
( f1 -- f2 )

Calculate cosh(x) where x is an S31.32 fixed-point number

##### `tanh`
( f1 -- f2 )

Calculate tanh(x) where x is an S31.32 fixed-point number

##### `asinh`
( f1 -- f2 )

Calculate asinh(x) where x is an S31.32 fixed-point number

##### `acosh`
( f1 -- f2 )

Calculate acosh(x) where x is an S31.32 fixed-point number

##### `atanh`
( f1 -- f2 )

Calculate atanh(x) where x is an S31.32 fixed-point number
