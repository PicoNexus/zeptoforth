\ Copyright (c) 2024 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

begin-module complex

  float32 import

  \ Domain error
  : x-domain-error ." domain error" cr ;
  
  \ Create an imaginary number
  : vimag ( f -- complex ) 0e0 ;

  \ Create a real number
  : vreal ( f -- complex ) 0e0 swap ;
  
  \ Add two complex numbers
  : cv+ { ai ar bi br -- ci cr } ai bi v+ ar br v+ ;

  \ Subtract two complex numbers
  : cv- { ai ar bi br -- ci cr } ai bi v- ar br v- ;

  \ Multiply two complex numbers
  : cv* { ai ar bi br -- ci cr }
    ai br v* ar bi v* v+ ar br v* ai bi v* v-
  ;

  \ Divide two complex number
  : cv/ { ai ar bi br -- ci cr }
    br dup v* bi dup v* v+ { den }
    ai br v* ar bi v* v- den v/ ar br v* ai bi v* v+ den v/
  ;

  \ Get the natural exponent of a complex number
  : cvexp { ai ar -- bi br }
    ar vexp { ar-vexp } ar-vexp ai vsin v* ar-vexp ai vcos v*
  ;

  \ Get the absolute value of a complex number
  : cvabs { ai ar -- f } ar dup v* ai dup v* v+ vsqrt ;

  \ Get the argument function of a complex number
  : cvarg { ai ar -- f }
    ai v0<> ar v0<> or averts x-domain-error
    ai ar vatan2
  ;

  \ Get the square root of a complex number
  : cvsqrt { D: a -- D: b }
    a cvabs vsqrt vreal a cvarg 2e0 v/ vimag cvexp cv*
  ;
  
  \ Get the principal value of the natural logarithm of a complex value
  : cvln { D: a -- D: b } a cvarg a cvabs vln ;

  \ The power function
  : cv** { D: c D: z -- D: cz** } c cvln z cv* cvexp ;
  
  \ The generalized power function, taking a value n due to the multivariant
  \ nature of the natural logarithm
  : cvn** { D: c D: z n -- D: cz** }
    c cvln n [ 2e0 vpi v* ] literal v* vimag cv+ z cv* cvexp
  ;
  
  \ Get the sine of a complex number
  : cvsin { D: a -- D: b }
    1e0 vimag a cv* { D: a' }
    -1e0 vreal a' cv* { D: a'' }
    a' cvexp a'' cvexp cv- 2e0 vimag cv/
  ;

  \ Get the cosine of a complex number
  : cvcos { D: a -- D: b }
    1e0 vimag a cv* { D: a' }
    -1e0 vreal a' cv* { D: a'' }
    a' cvexp a'' cvexp cv+ 2e0 vreal cv/
  ;

  \ Get the tangent of a complex number
  : cvtan { D: a -- D: b }
    1e0 vimag a cv* { D: a' }
    -1e0 vreal a' cv* { D: a'' }
    a' cvexp { D: a' }
    a'' cvexp { D: a'' }
    a' a'' cv- a' a'' cv+ cv/ -1e0 vimag cv*
  ;

  \ Get the principal value of the arcsine of a complex value
  : cvasin { D: a -- D: b }
    1e0 vreal a 2dup cv* cv- { D: a' }
    a' cvarg 2e0 v/ vexp a' cvabs 2e0 v/ v* vreal a 1e0 vimag cv* cv+ cvln
    1e0 vimag cv/
  ;

  \ Get the principal value of the arccosine of a complex value
  : cvacos { D: a -- D: b }
    1e0 vreal a 2dup cv* cv- { D: a' }
    a' cvarg 2e0 v/ vexp a' cvabs 2e0 v/ v* vreal 1e0 vimag cv* a cv+ cvln
    1e0 vimag cv/
  ;

  \ Get the principal value of the arctangent of a complex value
  : cvatan { D: a -- D: b }
    1e0 vimag a cv- 1e0 vimag a cv+ cv/ cvln 2e0 vimag cv/
  ;

  \ Get the principal value of the hyperbolic sine of a complex value
  : cvsinh { D: a -- D: b }
    a cvexp a -1e0 vreal cv* cvexp cv- 2e0 vreal cv/
  ;

  \ Get the principal value of the hyperbolic cosine of a complex value
  : cvcosh { D: a -- D: b }
    a cvexp a -1e0 vreal cv* cvexp cv+ 2e0 vreal cv/
  ;

  \ Get the principal value of the hyperbolic tangent of a complex value
  : cvtanh { D: a -- D: b }
    a cvexp { D: a' } a -1e0 vreal cv* cvexp { D: a'' }
    a' a'' cv- a' a a'' cv+ cv/
  ;

  \ Get the principal value of the hyperbolic arcsine of a complex value
  : cvasinh { D: a -- D: b }
    1e0 vreal a 2dup cv* cv+ { D: a' }
    a' cvarg 2e0 v/ vexp a' cvabs 2e0 v/ v* vreal a cv+ cvln
  ;

  \ Get the principal value of the hyperbolic arccosine of a complex value
  : cvacosh { D: a -- D: b }
    a 2dup cv* 1e0 vreal cv- { D: a' }
    a' cvarg 2e0 v/ vexp a' cvabs 2e0 v/ v* vreal a cv+ cvln
  ;

  \ Get the principal value of the hyperbolic arctangent of a complex value
  : cvatanh { D: a -- D: b }
    1e0 vreal a cv+ 1e0 vreal a cv- cv/ cvln 2e0 vreal cv/
  ;
  
end-module