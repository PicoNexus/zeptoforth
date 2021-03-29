\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist
forth-module 1 set-order
forth-module set-current
wordlist constant int-io-module
wordlist cosntant int-io-internal-module
forth-module internal-module interrupt-module int-io-module
int-io-internal-module 5 set-order
int-io-internal-module set-current

\ RAM variable for rx buffer read-index
bvariable rx-read-index

\ RAM variable for rx buffer write-index
bvariable rx-write-index

\ Constant for number of bytes to buffer
128 constant rx-buffer-size

\ Rx buffer
rx-buffer-size buffer: rx-buffer

\ RAM variable for tx buffer read-index
bvariable tx-read-index

\ RAM variable for tx buffer write-index
bvariable tx-write-index

\ Constant for number of bytes to buffer
128 constant tx-buffer-size

\ Tx buffer
tx-buffer-size buffer: tx-buffer

\ USART2
$4000200 constant UART_Base
UART_Base $108 + constant UART_EVENT_RXDRDY
UART_Base $11C + constant UART_EVENT_TXDRDY
UART_Base $304 + constant UART_INTENSET
UART_Base $308 + constant UART_INTENCLR
UART_Base $518 + constant UART_RXD
UART_Base $51C + constant UART_TXD
: UART_INTENSET_RXDRDY 1 2 lshift UART_INTENSET bis! ;
: UART_INTENCLR_RXDRDY 1 2 lshift UART_INTENCLR bis! ;
: UART_INTENSET_TXDRDY 1 7 lshift UART_INTENSET bis! ;
: UART_INTENCLR_TXDRDY 1 7 lshift UART_INTENCLR bis! ;

\ Get whether the rx buffer is full
: rx-full? ( -- f )
  rx-write-index b@ rx-read-index b@
  rx-buffer-size 1- + rx-buffer-size umod =
;

\ Get whether the rx buffer is empty
: rx-empty? ( -- f )
  rx-read-index b@ rx-write-index b@ =
;

\ Write a byte to the rx buffer
: write-rx ( c -- )
  rx-full? not if
    rx-write-index b@ rx-buffer + b!
    rx-write-index b@ 1+ rx-buffer-size mod rx-write-index b!
  else
    drop
  then
;

\ Read a byte from the rx buffer
: read-rx ( -- c )
  rx-empty? not if
    rx-read-index b@ rx-buffer + b@
    rx-read-index b@ 1+ rx-buffer-size mod rx-read-index b!
  else
    0
  then
;

\ Get whether the tx buffer is full
: tx-full? ( -- f )
  tx-write-index b@ tx-read-index b@
  tx-buffer-size 1- + tx-buffer-size umod =
;

\ Get whether the tx buffer is empty
: tx-empty? ( -- f )
  tx-read-index b@ tx-write-index b@ =
;

\ Write a byte to the tx buffer
: write-tx ( c -- )
  tx-full? not if
    tx-write-index b@ tx-buffer + b!
    tx-write-index b@ 1+ tx-buffer-size mod tx-write-index b!
  else
    drop
  then
;

\ Read a byte from the tx buffer
: read-tx ( -- c )
  tx-empty? not if
    tx-read-index b@ tx-buffer + b@
    tx-read-index b@ 1+ tx-buffer-size mod tx-read-index b!
  else
    0
  then
;

\ Handle IO
: handle-io ( -- )
  disable-int
  begin
    rx-full? not if
      UART_EVENT_RXDRDY @ if
	0 UART_EVENT_RXDRDY !
	UART_RXD b@ write-rx false
      else
	true
      then
    else
      true
    then
  until
  rx-full? if
    UART_INTENCLR_RXDRDY
  then
  begin
    tx-empty? not if
      UART_EVENT_TXDRDY @ if
	0 UART_EVENT_TXDRDY !
	read-tx UART_TXD b! false
      else
	true
      then
    else
      true
    then
  until
  tx-empty? if
    UART_INTENCLR_TXDRDY
  then
  enable-int
;

\ Null interrupt handler
: null-handler ( -- )
  handle-io
;

\ Interrupt-driven IO hooks

: do-emit ( c -- )
  [: tx-full? not ;] wait
  write-tx
  UART_INTENSET_TXDRDY
; 

: do-key ( -- c )
  UART_INTENSET_RXDRDY
  [: rx-empty? not ;] wait
  read-rx
;

: do-emit? ( -- flag )
  tx-full? not
;

: do-key? ( -- flag )
  rx-empty? not
;

\ Set non-internal
int-io-module set-current

\ Handle IO for multitasking
: task-io ( -- ) ;

\ Enable interrupt-driven IO
: enable-int-io ( -- )
  ['] null-handler null-handler-hook !
  ['] do-key key-hook !
  ['] do-emit emit-hook !
  ['] do-key? key?-hook !
  ['] do-emit? emit?-hook !
  UART_INTENSET_RXDRDY
;

\ Disable interrupt-driven IO
: disable-int-io ( -- )
  disable-int
  ['] serial-key key-hook !
  ['] serial-emit emit-hook !
  ['] serial-key? key?-hook !
  ['] serial-emit? emit?-hook !
  0 null-handler-hook !
  UART_INTENCLR_RXDRDY
  UART_INTENCLR_TXDRDY
  enable-int
;

\ Reset current wordlist
forth-module set-current

\ Init
: init ( -- )
  init
  0 rx-read-index b!
  0 rx-write-index b!
  0 tx-read-index b!
  0 tx-write-index b!
  enable-int-io
;

\ Reboot
reboot

