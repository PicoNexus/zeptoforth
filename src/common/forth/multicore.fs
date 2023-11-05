\ Copyright (c) 2020-2023 Travis Bemann
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

\ Compile to flash
compile-to-flash

begin-module multicore

  armv6m import

  \ Spinlock count
  0 constant spinlock-count

  \ Serial spinlock index
  -1 constant serial-spinlock

  \ Test and set spinlock index
  -1 constant test-set-spinlock

  \ Spinlock out of range exception
  : x-spinlock-out-of-range ( -- ) ." spinlock out of range" cr ;

  \ Core out of range exception
  : x-core-out-of-range ( -- ) ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) ." core not addressable" cr ;

  \ Just claim a spinlock - note that this is a no-op
  : claim-spinlock-raw ( index -- ) drop ;

  \ Just release a spinlock - note that this is a no-op
  : release-spinlock-raw ( index -- ) drop ;

  \ Claim a spinlock - note that this is a no-op
  : claim-spinlock ( index -- ) drop ;

  \ Release a spinlock - note that this is a no-op
  : release-spinlock ( index -- ) drop ;

  \ Just claim a spinlock for all cores' multitasker - this is a no-op
  : claim-all-core-spinlock-raw ( -- ) ;

  \ Just release a spinlock for all-cores multitasker - this is a no-op
  : release-all-core-spinlock-raw ( -- ) ;
  
  \ Claim a spinlock for the current core's multitasker - this is a no-op
  : claim-same-core-spinlock ( -- ) ;

  \ Release a spinlock for the current core's multitasker - this is a no-op
  : release-same-core-spinlock ( -- ) ;

  \ Claim a spinlock for a different core's multitasker - this is a no-op
  : claim-other-core-spinlock ( core -- ) drop ;

  \ Release a spinlock for the other core's multitasker - this is a no-op
  : release-other-core-spinlock ( core -- ) drop ;

  \ Claim all core's multitasker's spinlocks - this is a no-op
  : claim-all-core-spinlock ( -- ) ;

  \ Release all core's multitasker's spinlocks - this is a no-op
  : release-all-core-spinlock ( -- ) ;
  
  \ Execute an xt (and not claim a spinlock)
  : with-spinlock ( xt spinlock -- ) drop execute ;

  \ Enter a critical section (and not claim a spinlock)
  : critical-with-spinlock ( xt spinlock -- ) drop critical ;

  \ Enter a critical section (and not claim another core's multitasker's
  \ spinlock)
  : critical-with-other-core-spinlock ( xt core -- ) drop critical ;

  \ Exit a critical section and then re-enter it
  : outside-critical-with-other-core-spinlock ( xt core -- )
    drop outside-critical
  ;
  
  \ Enter a critical section (and not claim another core's multitasker's
  \ spinlock)
  : begin-critical-with-other-core-spinlock ( core -- )
    drop begin-critical
  ;

  \ Leave a critical section (and not release another core's multitasker's
  \ spinlock)
  : end-critical-with-other-core-spinlock ( core -- )
    drop end-critical
  ;

  \ Enter a critical section (and do not claim any spinlocks)
  : critical-with-all-core-spinlock ( xt -- ) critical ;

  \ Test and set
  : test-set ( value addr -- set? )
    code[
    cpsid
    r0 1 dp ldm
    0 tos r1 ldr_,[_,#_]
    0 r1 cmp_,#_
    ne bc>
    0 tos r0 str_,[_,#_]
    0 tos movs_,#_
    tos tos mvns_,_
    cpsie
    pc 1 pop
    >mark
    0 tos movs_,#_
    cpsie
    ]code
  ;
  
  \ Test and set without touching interrupts
  : test-set-raw ( value addr -- set? )
    code[
    r0 1 dp ldm
    0 tos r1 ldr_,[_,#_]
    0 r1 cmp_,#_
    ne bc>
    0 tos r0 str_,[_,#_]
    0 tos movs_,#_
    tos tos mvns_,_
    pc 1 pop
    >mark
    0 tos movs_,#_
    ]code
  ;

  \ Drain a multicore FIFO
  : fifo-drain ( core -- ) ['] x-core-out-of-range ?raise ;
  
  \ Blocking FIFO push
  : fifo-push-blocking ( x core -- ) ['] x-core-out-of-range ?raise ;
  
  \ Blocking FIFO pop
  : fifo-pop-blocking ( core -- x ) ['] x-core-out-of-range ?raise ;

  \ Attempt to send data on a FIFO and confirm that the same data is sent back.
  : fifo-push-confirm ( x core -- confirmed? ) ['] x-core-out-of-range ?raise ;
  
  \ Launch an auxiliary core
  : launch-aux-core ( xt stack-ptr rstack-ptr core -- )
    ['] x-core-out-of-range ?raise
  ;

  \ Reset an auxiliary core
  : reset-aux-core ( core -- ) ['] x-core-out-of-range ?raise ;

end-module

\ Reboot
reboot
