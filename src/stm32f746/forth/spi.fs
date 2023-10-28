\ Copyright (c) 2022-2023 Travis Bemann
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

compile-to-flash

begin-module spi

  interrupt import
  multicore import
  pin import
  
  \ Invalid SPI periperal exception
  : x-invalid-spi ( -- ) ." invalid SPI" cr ;

  \ Invalid SPI clock setting
  : x-invalid-spi-clock ( -- ) ." invalid SPI clock" cr ;

  \ Invalid SPI data size setting
  : x-invalid-spi-data-size ( -- ) ." invalid SPI data size" cr ;

  begin-module spi-internal

    \ RX buffer size in bytes (not halfwords)
    128 constant spi-rx-buffer-size
    
    \ TX buffer size in bytes (not halfwords)
    128 constant spi-tx-buffer-size

    \ SPI peripheral count
    6 constant spi-count
    
    \ Validate an SPI peripheral
    : validate-spi ( spi -- )
      dup 1 u>= averts x-invalid-spi
      spi-count 1+ u< averts x-invalid-spi
    ;

    begin-structure spi-size

      \ RX buffer read-index
      cfield: spi-rx-read-index
      
      \ RX buffer write-index
      cfield: spi-rx-write-index
      
      \ RX buffer
      spi-rx-buffer-size +field spi-rx-buffer
      
      \ TX buffer read-index
      cfield: spi-tx-read-index
      
      \ TX buffer write-index
      cfield: spi-tx-write-index
      
      \ TX buffer
      spi-tx-buffer-size +field spi-tx-buffer

      \ Receiver handler
      field: spi-rx-handler

      \ Eight byte DR
      field: spi-8-bit-dr

    end-structure

    spi-size spi-count * buffer: spi-buffers

    \ RCC base address
    $40023800 constant RCC_Base

    \ RCC registers
    RCC_Base $40 + constant RCC_APB1ENR ( RCC_APB1ENR )
    RCC_Base $44 + constant RCC_APB2ENR ( RCC_APB2ENR )
    RCC_Base $60 + constant RCC_APB1LPENR ( RCC_APB1LPENR )
    RCC_Base $64 + constant RCC_APB2LPENR ( RCC_APB2LPENR )

    \ RCC fields
    : RCC_APB1ENR_SPI2EN 14 bit RCC_APB1ENR bis! ;
    : RCC_APB1ENR_SPI2EN_Clear 14 bit RCC_APB1ENR bic! ;
    : RCC_APB1ENR_SPI3EN 15 bit RCC_APB1ENR bis! ;
    : RCC_APB1ENR_SPI3EN_Clear 15 bit RCC_APB1ENR bic! ;
    : RCC_APB2ENR_SPI1EN 12 bit RCC_APB2ENR bis! ;
    : RCC_APB2ENR_SPI1EN_Clear 12 RCC_APB2ENR bic! ;
    : RCC_APB2ENR_SPI4EN 13 bit RCC_APB2ENR bis! ;
    : RCC_APB2ENR_SPI4EN_Clear 13 RCC_APB2ENR bic! ;
    : RCC_APB2ENR_SPI5EN 20 bit RCC_APB2ENR bis! ;
    : RCC_APB2ENR_SPI5EN_Clear 20 RCC_APB2ENR bic! ;
    : RCC_APB2ENR_SPI6EN 21 bit RCC_APB2ENR bis! ;
    : RCC_APB2ENR_SPI6EN_Clear 21 RCC_APB2ENR bic! ;
    : RCC_APB1LPENR_SPI2LPEN 14 bit RCC_APB1LPENR bis! ;
    : RCC_APB1LPENR_SPI2LPEN_Clear 14 bit RCC_APB1LPENR bic! ;
    : RCC_APB1LPENR_SPI3LPEN 15 bit RCC_APB1LPENR bis! ;
    : RCC_APB1LPENR_SPI3LPEN_Clear 15 bit RCC_APB1LPENR bic! ;
    : RCC_APB2LPENR_SPI1LPEN 12 bit RCC_APB2LPENR bis! ;
    : RCC_APB2LPENR_SPI1LPEN_Clear 12 RCC_APB2LPENR bic! ;
    : RCC_APB2LPENR_SPI4LPEN 13 bit RCC_APB2LPENR bis! ;
    : RCC_APB2LPENR_SPI4LPEN_Clear 13 RCC_APB2LPENR bic! ;
    : RCC_APB2LPENR_SPI5LPEN 20 bit RCC_APB2LPENR bis! ;
    : RCC_APB2LPENR_SPI5LPEN_Clear 20 RCC_APB2LPENR bic! ;
    : RCC_APB2LPENR_SPI6LPEN 21 bit RCC_APB2LPENR bis! ;
    : RCC_APB2LPENR_SPI6LPEN_Clear 21 RCC_APB2LPENR bic! ;

    \ SPI base register table
    create SPI_Base_table
    $40013000 ,
    $40003800 ,
    $40003C00 ,
    $40013400 ,
    $40015000 ,
    $40015400 ,
    
    \ SPI base
    : SPI_Base ( spi -- addr ) 1- cells SPI_Base_table + @ ;

    \ SPI registers
    : SPI_CR1 ( spi -- addr ) SPI_Base $000 + ;
    : SPI_CR2 ( spi -- addr ) SPI_Base $004 + ;
    : SPI_SR ( spi -- addr ) SPI_Base $008 + ;
    : SPI_DR ( spi -- addr ) SPI_Base $00C + ;
    : SPI_CRCPR ( spi -- addr ) SPI_Base $010 + ;
    : SPI_RXCRCR ( spi -- addr ) SPI_Base $014 + ;
    : SPI_TXCRCR ( spi -- addr ) SPI_Base $018 + ;

    \ SPI fields
    : SPI_CR1_BIDIMODE! ( bidimode spi -- )
      15 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_BIDIOE! ( bidioe spi -- )
      14 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_CRCEN! ( crcen spi -- )
      13 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_CRCNEXT! ( crcnext spi -- )
      12 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_CRCL! ( crcl spi -- )
      11 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_RXONLY! ( rxonly spi -- )
      10 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_SSM! ( ssm spi -- )
      9 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_SSI! ( ssi spi -- )
      8 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_LSBFIRST! ( lsbfirst spi -- )
      7 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_SPE! ( spe spi -- )
      6 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_BR! ( br spi -- )
      SPI_CR1 dup >r @ $38 bic swap $7 and 3 lshift or r> !
    ;
    : SPI_CR1_MSTR! ( mstr spi -- )
      2 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_CPOL! ( cpol spi -- )
      1 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR1_CPHA! ( cpha spi -- )
      0 bit swap SPI_CR1 rot if bis! else bic! then
    ;
    : SPI_CR2_DS! ( ds spi -- )
      SPI_CR2 dup >r @ $F00 bic swap $F and 8 lshift or r> !
    ;
    : SPI_CR2_FRXTH! ( frxth spi -- )
      12 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_CR2_TXEIE! ( txeie spi -- )
      7 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_CR2_RXNEIE! ( rxneie spi -- )
      6 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_CR2_ERRIE! ( errie spi -- )
      5 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_CR2_FRF! ( frf spi -- )
      4 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_CR2_SSOE! ( ssoe spi -- )
      2 bit swap SPI_CR2 rot if bis! else bic! then
    ;
    : SPI_SR_FRE@ ( spi -- fre ) 8 bit swap SPI_SR bit@ ;
    : SPI_SR_BSY@ ( spi -- bsy ) 7 bit swap SPI_SR bit@ ;
    : SPI_SR_OVR@ ( spi -- ovr ) 6 bit swap SPI_SR bit@ ;
    : SPI_SR_MODF@ ( spi -- modf ) 5 bit swap SPI_SR bit@ ;
    : SPI_SR_CRCERR@ ( spi -- crcerr ) 4 bit swap SPI_SR bit@ ;
    : SPI_SR_TXE@ ( spi -- txe ) 1 bit swap SPI_SR bit@ ;
    : SPI_SR_RXNE@ ( spi -- rxne ) 0 bit swap SPI_SR bit@ ;

    \ SPI IRQ table
    create spi-irq-table 35 h, 36 h, 51 h, 84 h, 85 h, 86 h,
    
    \ SPI IRQ
    : spi-irq ( spi -- addr ) 1- 2* spi-irq-table + h@ ;
    
    \ SPI vector
    : spi-vector ( spi -- vector ) spi-irq 16 + ;
    
    \ Select SPI structure
    : spi-select ( spi -- addr ) 1- spi-size * spi-buffers + ;

    \ RX buffer read-index
    : spi-rx-read-index ( spi -- addr ) spi-select spi-rx-read-index ;
      
    \ RX buffer write-index
    : spi-rx-write-index ( spi -- addr ) spi-select spi-rx-write-index ;

    \ RX buffer
    : spi-rx-buffer ( spi -- addr ) spi-select spi-rx-buffer ;
      
    \ TX buffer read-index
    : spi-tx-read-index ( spi -- addr ) spi-select spi-tx-read-index ;
      
    \ TX buffer write-index
    : spi-tx-write-index ( spi -- addr ) spi-select spi-tx-write-index ;

    \ TX buffer
    : spi-tx-buffer ( spi -- addr ) spi-select spi-tx-buffer ;

    \ SPI RX handler
    : spi-rx-handler ( spi -- addr ) spi-select spi-rx-handler ;

    \ SPI 8-bit DR access
    : spi-8-bit-dr ( spi -- addr ) spi-select spi-8-bit-dr ;
    
    \ Get whether the rx buffer is full
    : spi-rx-full? ( spi -- f )
      dup spi-rx-write-index c@ swap spi-rx-read-index c@
      spi-rx-buffer-size 1- + spi-rx-buffer-size umod =
    ;

    \ Get whether the rx buffer is empty
    : spi-rx-empty? ( spi -- f )
      dup spi-rx-read-index c@ swap spi-rx-write-index c@ =
    ;

    \ Write a halfword to the rx buffer
    : spi-write-rx ( h spi -- )
      dup spi-rx-full? not if
	tuck dup spi-rx-write-index c@ swap spi-rx-buffer + h!
	dup spi-rx-write-index c@ 2 + spi-rx-buffer-size mod
	swap spi-rx-write-index c!
      else
	2drop
      then
    ;

    \ Read a halfword from the rx buffer
    : spi-read-rx ( spi -- h )
      dup spi-rx-empty? not if
	dup spi-rx-read-index c@ over spi-rx-buffer + h@
	over spi-rx-read-index c@ 2 + spi-rx-buffer-size mod
	rot spi-rx-read-index c!
      else
	drop 0
      then
    ;

    \ Get whether the tx buffer is full
    : spi-tx-full? ( spi -- f )
      dup spi-tx-write-index c@ swap spi-tx-read-index c@
      spi-tx-buffer-size 1- + spi-tx-buffer-size umod =
    ;

    \ Get whether the tx buffer is empty
    : spi-tx-empty? ( spi -- f )
      dup spi-tx-read-index c@ swap spi-tx-write-index c@ =
    ;

    \ Write a halfword to the tx buffer
    : spi-write-tx ( h spi -- )
      dup spi-tx-full? not if
	tuck dup spi-tx-write-index c@ swap spi-tx-buffer + h!
	dup spi-tx-write-index c@ 2 + spi-tx-buffer-size mod
	swap spi-tx-write-index c!
      else
	2drop
      then
    ;

    \ Read a halfword from the tx buffer
    : spi-read-tx ( spi -- h )
      dup spi-tx-empty? not if
	dup spi-tx-read-index c@ over spi-tx-buffer + h@
	over spi-tx-read-index c@ 2 + spi-tx-buffer-size mod
	rot spi-tx-read-index c!
      else
	drop 0
      then
    ;

    \ Write a frame
    : >frame { spi -- }
      spi spi-8-bit-dr @ if
        spi SPI_DR c!
      else
        spi SPI_DR h!
      then
    ;

    \ Read a frame
    : frame> { spi -- }
      spi spi-8-bit-dr @ if
        spi SPI_DR c@
      else
        spi SPI_DR h@
      then
    ;

    \ Handle an SPI interrupt
    : handle-spi ( spi -- )
      [:
	begin
	  dup spi-tx-empty? not if
	    dup SPI_SR_TXE@ if
	      dup spi-read-tx over >frame false
	    else
	      true
	    then
	  else
	    true
	  then
	until
	false
	begin
	  over spi-rx-full? not if
	    over SPI_SR_RXNE@ if
	      over frame> 2 pick spi-write-rx drop true false
	    else
	      true
	    then
	  else
	    true
	  then
	until
      ;] serial-spinlock critical-with-spinlock
      if dup spi-rx-handler @ ?execute then
      dup spi-tx-empty? not swap SPI_CR2_TXEIE!
    ;

    \ Handle an SPI interrupt for SPI1
    : handle-spi1 1 handle-spi ;

    \ Handle an SPI interrupt for SPI2
    : handle-spi2 2 handle-spi ;

    \ Handle an SPI interrupt for SPI3
    : handle-spi3 3 handle-spi ;

    \ Handle an SPI interrupt for SPI4
    : handle-spi4 4 handle-spi ;

    \ Handle an SPI interrupt for SPI5
    : handle-spi5 5 handle-spi ;

    \ Handle an SPI interrupt for SPI6
    : handle-spi6 6 handle-spi ;

    \ Initialize an SPI entity
    : init-spi ( spi -- )
      disable-int
      0 over spi-rx-read-index c!
      0 over spi-rx-write-index c!
      0 over spi-tx-read-index c!
      0 over spi-tx-write-index c!
      0 over spi-irq NVIC_IPR_IP!
      0 over spi-rx-handler !
      false over spi-8-bit-dr !
      dup case
	1 of RCC_APB2ENR_SPI1EN RCC_APB2LPENR_SPI1LPEN endof
	2 of RCC_APB1ENR_SPI2EN RCC_APB1LPENR_SPI2LPEN endof
	3 of RCC_APB1ENR_SPI3EN RCC_APB1LPENR_SPI3LPEN endof
	4 of RCC_APB2ENR_SPI4EN RCC_APB2LPENR_SPI4LPEN endof
	5 of RCC_APB2ENR_SPI5EN RCC_APB2LPENR_SPI5LPEN endof
	6 of RCC_APB2ENR_SPI6EN RCC_APB2LPENR_SPI6LPEN endof
      endcase
      dup case
	1 of ['] handle-spi1 endof
	2 of ['] handle-spi2 endof
	3 of ['] handle-spi3 endof
	4 of ['] handle-spi4 endof
	5 of ['] handle-spi5 endof
	6 of ['] handle-spi6 endof
      endcase
      over spi-vector vector!
      true over SPI_CR2_RXNEIE!
      spi-irq NVIC_ISER_SETENA!
      enable-int
    ;

  end-module> import

  \ Set the SPI baud
  : spi-baud! { baud spi -- }
    spi validate-spi
    spi case
      1 of 108000000 endof
      2 of 54000000 endof
      3 of 54000000 endof
      4 of 108000000 endof
      5 of 108000000 endof
      6 of 108000000 endof
    endcase { max-baud }
    1 8 do
      max-baud i rshift baud >= if
	i 1- spi SPI_CR1_BR! unloop exit
      then
    -1 +loop
    0 spi SPI_CR1_BR!
  ;

  \ Set SPI to master
  : master-spi ( spi -- ) dup validate-spi true swap SPI_CR1_MSTR! ;

  \ Set SPI to salve
  : slave-spi ( spi -- ) dup validate-spi false swap SPI_CR1_MSTR! ;

  \ Set SPI to Motorola SPI frame format with SPH and SPO settings
  : motorola-spi ( sph spo spi -- )
    dup validate-spi
    tuck SPI_CR1_CPOL!
    tuck SPI_CR1_CPHA!
    false swap SPI_CR2_FRF!
  ;

  \ Set SPI to TI synchronous serial frame format
  : ti-ss-spi ( spi -- ) dup validate-spi true swap SPI_CR2_FRF! ;

  \ Set SPI to MSB-first (not needed in TI mode)
  : msb-first-spi ( spi -- )
    dup validate-spi
    false swap SPI_CR1_LSBFIRST!
  ;

  \ Set SPI to LSB-first (not needed in TI mode)
  : lsb-first-spi ( spi -- )
    dup validate-spi
    true swap SPI_CR1_LSBFIRST!
  ;
  
  \ Set SPI data size
  : spi-data-size! ( data-size spi -- )
    dup validate-spi swap
    dup 4 u>= averts x-invalid-spi-data-size
    dup 16 u<= averts x-invalid-spi-data-size
    dup 8 <= 2 pick SPI_CR2_FRXTH!
    dup 8 <= 2 pick spi-8-bit-dr !
    1- swap SPI_CR2_DS!
  ;

  \ Enable software slave management
  : enable-spi-ssm ( spi -- ) dup validate-spi true swap SPI_CR1_SSM! ;
    
  \ Disable software slave managemnt
  : disable-spi-ssm ( spi -- ) dup validate-spi false swap SPI_CR1_SSM! ;

  \ Set internal slave select
  : spi-ssm! ( ssi spi -- ) dup validate-spi SPI_CR1_SSI! ;

  \ Enable NSS output
  : enable-spi-nss ( spi -- ) dup validate-spi true swap SPI_CR2_SSOE! ;

  \ Disable NSS output
  : disable-spi-nss ( spi -- ) dup validate-spi false swap SPI_CR2_SSOE! ;
  
  \ Enable SPI
  : enable-spi ( spi -- ) dup validate-spi true swap SPI_CR1_SPE! ;

  \ Disable SPI
  : disable-spi ( spi -- ) dup validate-spi false swap SPI_CR1_SPE! ;

  \ Enable SPI TX
  : enable-spi-tx ( spi -- ) dup validate-spi false swap SPI_CR1_RXONLY! ;

  \ Disable SPI TX for slaves
  : disable-spi-tx ( spi -- ) dup validate-spi true swap SPI_CR1_RXONLY! ;

  \ Enable 1-line SPI
  : 1-line-spi ( spi -- ) dup validate-spi true swap SPI_CR1_BIDIMODE! ;

  \ Enable 2-line SPI
  : 2-line-spi ( spi -- ) dup validate-spi false swap SPI_CR1_BIDIMODE! ;

  \ 1-line SPI in mode
  : 1-line-spi-in ( spi -- ) dup validate-spi false swap SPI_CR1_BIDIOE! ;
  
  \ 1-line SPI out mode
  : 1-line-spi-out ( spi -- ) dup validate-spi true swap SPI_CR1_BIDIOE! ;

  \ Set the SPI RX handler; note that this handler executes in an interrupt
  \ handler
  : spi-rx-handler! ( xt spi -- ) dup validate-spi spi-rx-handler ! ;

  \ SPI alternate function
  : spi-alternate ( spi -- alternate )
    dup validate-spi
    case
      1 of 5 endof
      2 of 5 endof
      3 of 6 endof
      4 of 5 endof
      5 of 5 endof
      6 of 5 endof
    endcase
  ;

  \ Set a pin to be an SPI pin
  : spi-pin ( spi pin -- )
    over validate-spi
    dup pin-internal::validate-pin
    2dup 6 xd = swap 3 = and if
      nip 5 swap
    else
      2dup 2 xb = swap 3 = and if
	nip 7 swap
      else
	2dup 4 xb = swap 2 = and if
	  nip 7 swap
	else
	  swap spi-alternate swap
	then
      then
    then
    dup fast-pin alternate-pin
  ;

  \ Write a halfword to SPI
  : >spi ( h spi -- )
    dup validate-spi
    [:
      dup spi-tx-empty? if
	dup SPI_SR_TXE@ not if
	  dup spi-tx-full? not if
	    tuck spi-write-tx
	    true swap SPI_CR2_TXEIE!
	  then
	else
	  >frame
	then
      else
	dup spi-tx-full? not if
	  tuck spi-write-tx
	  true swap SPI_CR2_TXEIE!
	then
      then
    ;] serial-spinlock critical-with-spinlock
  ;

  \ Read a halfword from SPI
  : spi> ( spi -- h )
    dup validate-spi
    begin
      [:
	disable-int
	dup spi-rx-empty? if
	  dup SPI_SR_RXNE@ if
	    frame> enable-int false true
	  else
	    enable-int false
	  then
	else
	  enable-int true true
	then
      ;] serial-spinlock critical-with-spinlock
      dup not if pause then
    until
    if spi-read-rx then
  ;

  \ Get whether an SPI peripheral can have data written
  : >spi? ( spi -- tx? ) dup validate-spi spi-tx-full? not ;

  \ Get whether there is data to receive in the SPI FIFO's
  : spi>? ( spi -- rx? )
    dup validate-spi dup spi-rx-empty? not swap SPI_SR_RXNE@ or
  ;

  \ Flush SPI
  : flush-spi ( spi -- )
    dup validate-spi
    begin dup spi-tx-empty? not while pause repeat
    begin dup SPI_SR_BSY@ while repeat drop
  ;

  \ Drain SPI
  : drain-spi ( spi -- )
    dup validate-spi
    begin dup spi>? while dup spi> drop repeat
    begin dup SPI_SR_BSY@ while repeat
    begin dup spi>? while dup spi> drop repeat drop
  ;
  
  \ Write a buffer to SPI
  : buffer>spi { buffer bytes spi -- }
    spi validate-spi
    spi drain-spi
    spi flush-spi
    spi spi-irq NVIC_ICER_CLRENA!
    spi spi-8-bit-dr @ { byte }
    byte if 1 else 2 then { unit }
    0 { tx-offset }
    bytes unit align unit / { rx-count }
    spi SPI_DR { port }
    spi SPI_SR { flags }
    begin tx-offset bytes < rx-count 0> or while
      flags @ [ 1 bit ] literal and if
        tx-offset bytes < if
          buffer tx-offset + byte if c@ port c! 1 else h@ port h! 2 then
          +to tx-offset
        then
      then
      flags @ [ 0 bit ] literal and if
        rx-count if
          byte if port c@ else port h! then drop
          -1 +to rx-count
        then
      then
    repeat
    spi spi-irq NVIC_ISER_SETENA!
  ;

  \ Read a buffer from SPI
  : spi>buffer { buffer bytes filler spi -- }
    spi validate-spi
    spi drain-spi
    spi flush-spi
    spi spi-irq NVIC_ICER_CLRENA!
    spi spi-8-bit-dr @ { byte }
    byte if 1 else 2 then { unit }
    0 { rx-offset }
    bytes unit align unit / { tx-count }
    spi SPI_DR { port }
    spi SPI_SR { flags }
    begin tx-count 0> rx-offset bytes < or while
      flags @ [ 1 bit ] literal and if
        tx-count if
          filler port byte if c! else h! then
          -1 +to tx-count
        then
      then
      flags @ [ 0 bit ] literal and if
        rx-offset bytes < if
          byte if
            port c@ buffer rx-offset + c! 1
          else
            port h@ buffer rx-offset + h! 2
          then
          +to rx-offset
        then
      then
    repeat
    spi spi-irq NVIC_ISER_SETENA!
  ;

end-module> import

\ Initialize
: init ( -- )
  init
  1 spi-internal::init-spi
  2 spi-internal::init-spi
  3 spi-internal::init-spi
  4 spi-internal::init-spi
  5 spi-internal::init-spi
  6 spi-internal::init-spi
;

reboot
