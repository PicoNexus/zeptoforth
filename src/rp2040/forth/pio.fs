\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module-once pio-module
  
  \ PIO0 base register
  $50200000 constant PIO0
  
  \ PIO1 base register
  $50300000 constant PIO1

  \ PIO control register
  : CTRL ( pio -- addr ) [inlined] $000 + ;
  
  \ Restart state machines' clock dividers LSB
  8 constant CTRL_CLKDIV_RESTART_LSB

  \ Restart state machines' clock dividers mask
  $F 8 lshift constant CTRL_CLKDIV_RESTART_MASK

  \ Clear state machine internal state LSB
  4 constant CTRL_SM_RESTART_LSB

  \ Clear state machine internal state mask
  $F 4 lshift constant CTRL_SM_RESTART_MASK

  \ Enable/disable state machines LSB
  0 constant CTRL_SM_ENABLE_LSB

  \ Enable/disable state machines mask
  $F 0 lshift constant CTRL_SM_ENABLE_MASK
  
  \ FIFO status register
  : FSTAT ( pio -- addr ) [inlined] $004 + ;

  \ State machine TX FIFO is empty LSB
  24 constant FSTAT_TXEMPTY_LSB

  \ State machine TX FIFO is empty mask
  $F 24 lshift constant FSTAT_TXEMPTY_MASK

  \ State machine TX FIFO is full LSB
  16 constant FSTAT_TXFULL_LSB

  \ State machine TX FIFO is full mask
  $F 16 lshift constant FSTAT_TXFULL_MASK

  \ State machine RX FIFO is empty LSB
  8 constant FSTAT_RXEMPTY_LSB

  \ State machine RX FIFO is empty mask
  $F 8 lshift constant FSTAT_RXEMPTY_MASK

  \ State machine RX FIFO is full LSB
  0 constant FSTAT_RXFULL_LSB

  \ State machine RX FIFO is full mask
  $F 0 lshift constant FSTAT_RXFULL_MASK
  
  \ FIFO debug register
  : FDEBUG ( pio -- addr ) [inlined] $008 + ;
  
  \ State machine has stalled on empty TX FIFO during a blocking pull or an
  \ OUT with autopull enabled. Write 1 to clear.
  24 constant FDEBUG_TXSTALL_LSB

  \ State machine has stalled on empty TX FIFO during a blocking pull or an
  \ OUT with autopull enabled. Write 1 to clear.
  $F 24 lshift constant FDEBUG_TXSTALL_MASK

  \ State machine TX FIFO overflow has occurred. Write 1 to clear.
  16 constant FDEBUG_TXOVER_LSB

  \ State machine TX FIFO overflow has occurred. Write 1 to clear.
  $F 16 lshift constant FDEBUG_TXOVER_MASK

  \ State machine RX FIFO underflow has occurred. Write 1 to clear.
  8 constant FDEBUG_RXUNDER_LSB

  \ State machine RX FIFO underflow has occurred. Write 1 to clear.
  $F 8 lshift constant FDEBUG_RXUNDER_MASK

  \ State machine has stalled on a full RX FIFO during a blocking PUSH or
  \ an IN with autopush enabled. Write 1 to clear.
  0 constant FDEBUG_RXSTALL_LSB

  \ State machine has stalled on a full RX FIFO during a blocking PUSH or
  \ an IN with autopush enabled. Write 1 to clear.
  $F 0 lshift constant FDEBUG_RXSTALL_MASK

  \ FIFO levels
  : FLEVEL ( pio -- addr ) [inlined] $00C + ;

  \ RX FIFO levels
  : FLEVEL_RX_LSB ( index -- lsb ) [inlined] 8 * 4 + ;

  \ RX FIFO masks
  : FLEVEL_RX_MASK ( index -- mask ) [inlined] $F swap FLEVEL_RX_LSB lshift ;

  \ TX FIFO levels
  : FLEVEL_TX_LSB ( index -- lsb ) [inlined] 8 * 4 + ;

  \ TX FIFO masks
  : FLEVEL_TX_MASK ( index -- mask ) [inlined] $F swap FLEVEL_TX_LSB lshift ;

  \ Direct write access to the TX FIFO for this state machine
  : TXF ( state-machine pio -- addr ) [inlined] swap cells + $010 + ;

  \ Direct read access to the RX FIFO for this state machine
  : RXF ( state-machine pio -- addr ) [inlined] swap cells + $020 + ;

  \ IRQ register (write 1 to clear)
  : IRQ ( pio -- addr ) [inlined] $030 + ;

  \ IRC force register (writing one to each of the bits forcibly asserts the
  \ IRQ for the PIO state machine)
  : IRQ_FORCE ( pio -- addr ) [inlined] $034 + ;

  \ Each bit corresponds to the state of a 2-flipflop synchronizer on a GPIO
  \ input; 0 (the default) corresponds to synchronization and 1 bypasses;
  \ when in doubt leave as 0.
  : INPUT_SYNC_BYPASS ( pio -- addr ) [inlined] $038 + ;

  \ Get the pad output values the PIO is driving for the GPIO's
  : DBG_PADOUT ( pio -- addr ) [inlined] $03C + ;

  \ Get the pad output enable values the PIO is drivng for the GPIO's
  : DBG_PADOE ( pio -- addr ) [inlined] $040 + ;

  \ Configuration info
  : DBG_CFGINFO ( pio -- addr ) [inlined] $044 + ;

  \ LSB of the size of the instruction memory in instructions
  16 constant DBG_CFGINFO_IMEM_SIZE_LSB

  \ Mask of the size of instruction memory in instructions
  $3F 16 lshift constant DBG_CFGINFO_IMEM_SIZE_MASK

  \ LSB of the number of state machines this PIO instance supports
  8 constant DBG_CFGINFO_SM_COUNT_LSB

  \ Mask of the number of state machines this PIO instance supports
  $F 8 lshift constant DBG_CFGINFO_SM_COUNT_MASK

  \ LSB of the depth of the state machine TX/RX FIFO's in words
  0 constant DBG_CFGINFO_FIFO_DEPTH_LSB

  \ Mask of the depth of the state machine TX/RX FIFO's in words
  $3F 0 lshift constant DBG_CFGINFO_FIFO_DEPTH_MASK
  
  \ Write only instruction memory (32 words in all)
  : INSTR_MEM ( index pio -- addr ) [inlined] $048 + + ;

  \ Clock divisor register for state machines
  : SM_CLKDIV ( state-machine pio -- addr ) [inlined] swap $18 * + $0C8 + ;

  \ Execution/behavioral settings for state machines
  : SM_EXECCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0CC + ;

  \ Control behavior of the input/output shift registers for state machines
  : SM_SHIFTCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0D0 + ;

  \ Current instruction address of state machines
  : SM_ADDR ( state-machine pio -- addr ) [inlined] swap $18 * + $0D4 + ;

  \ Read to see the instruction currently addressed by state machines' program
  \ counters, write to execute an instruction immediately
  : SM_INSTR ( state-machine pio -- addr ) [inlined] swap $18 * + $0D8 + ;

  \ State machine pin control
  : SM_PINCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0DC + ;

  \ Interrupt indices
  : INT_SM ( state-machine -- index ) [inlined] 8 + ;

  \ TXN full interupt indices
  : INT_SM_TXNFULL ( state-machine -- index ) [inlined] 4 + ;

  \ RXN full interrupt indices
  : INT_SM_RXNFULL ( state-machine -- index ) [inlined] ;

  \ IRQ0
  0 constant IRQ0

  \ IRQ1
  1 constant IRQ1
  
  \ Raw interrupts
  : INTR ( pio -- addr ) [inlined] $128 + ;

  \ Interrupt enable registers
  : INTE ( irq pio -- addr ) [inlined] $12C + swap $0C * + ;

  \ Interrupt force regisers
  : INTF ( irq pio -- addr ) [inlined] $130 + swap $0C * + ;

  \ Interrupt status registers
  : INTS ( irq pio -- addr ) [inlined] $134 + swap $0C * + ;

  \ Add the state machine ID to the lower two bits of the IRQ index, by way of
  \ module-4 addition on the two LSB's.
  : REL %10000 or ;

  \ Always jump
  %000 constant COND_ALWAYS

  \ Jump if scratch X is zero
  %001 constant COND_X0=

  \ Jump if scratch X is non-zero, post-decrement
  %010 constant COND_X1-

  \ Jump if scratch Y is zero
  %011 constant COND_Y0=

  \ Jump if scratch Y is non-zero, post-decrement
  %100 constant COND_Y1-

  \ Jump if scratch X not equal scratch Y
  %101 constant COND_XY<>

  \ Jump on input pin
  %110 constant COND_PIN

  \ Jump on output shift register not empty
  %111 constant COND_IOSRE

  \ Wait for GPIO
  %00 constant WAIT_GPIO

  \ Wait for a pin
  %01 constant WAIT_PIN

  \ Wait for an IRQ
  %10 constant WAIT_IRQ

  \ Pins input
  %000 constant IN_PINS

  \ Scratch register X input
  %001 constant IN_X

  \ Scratch register Y input
  %010 constant IN_Y

  \ NULL input (all zeros)
  %011 constant IN_NULL

  \ ISR input
  %110 constant IN_ISR

  \ OSR input
  %111 constant IN_OSR

  \ Pins output
  %000 constant OUT_PINS

  \ Scratch register X output
  %001 constant OUT_X

  \ Scratch register Y output
  %010 constant OUT_Y

  \ NULL output (discard data)
  %011 constant OUT_NULL

  \ PINDIRs output
  %100 constant OUT_PINDIRS

  \ PC output (unconditional jump to shifted address )
  %101 constant OUT_PC

  \ ISR output (also sets ISR shift counter to bit count)
  %110 constant OUT_ISR

  \ Execute OSR shift data as instruction
  %111 constant OUT_EXEC

  \ Push data even if threshold is not met
  false constant PUSH_NOT_FULL

  \ Do nothing unless the total input shift count has reached its threshold
  true constant PUSH_IF_FULL

  \ Do not stall execution if RX FIFO is full, instead drop data from ISR
  false constant PUSH_NO_BLOCK
  
  \ Stall execution if RX FIFO is full.
  true constant PUSH_BLOCK

  \ Pull data even if threshold is not met
  false constant PULL_NOT_EMPTY

  \ Do nothing unless the total output shift count has reached its threshold
  true constant PULL_IF_EMPTY

  \ Do not stall execution if TX FIFO is empty, instead copy from scratch X
  false constant PULL_NO_BLOCK

  \ Stall execution if TX FIFO is empty
  true constant PULL_BLOCK

  \ Move to PINS
  %000 constant MOV_DEST_PINS

  \ Move to scratch register X
  %001 constant MOV_DEST_X

  \ Move to scratch register Y
  %010 constant MOV_DEST_Y

  \ Move to EXEC (execute data as instruction)
  %100 constant MOV_DEST_EXEC

  \ Move to PC (treat data as address for unconditional branch)
  %101 constant MOV_DEST_PC

  \ Move to ISR (input shift counter is reset to 0, i.e. empty)
  %110 constant MOV_DEST_ISR

  \ Move to OSR (input shift counter is reset to 0, i.e. full)
  %111 constant MOV_DEST_OSR

  \ Move operation none
  %00 constant MOV_OP_NONE

  \ Move operation invert
  %01 constant MOV_OP_INVERT

  \ Move operation bit-reverse
  %10 constant BIT_OP_REVERSE

  \ Move from PINS
  %000 constant MOV_SRC_PINS

  \ Move from scratch register X
  %001 constant MOV_SRC_X

  \ Move from scratch register Y
  %010 constant MOV_SRC_Y

  \ Move from NULL
  %011 constant MOV_SRC_NULL

  \ Move from STATUS
  %101 constant MOV_SRC_STATUS

  \ Move from ISR
  %110 constant MOV_SRC_ISR

  \ Move from OSR
  %111 constant MOV_SRC_OSR

  \ Raise an IRQ
  %00 constant IRQ_SET

  \ Clear an IRQ
  %10 constant IRQ_CLEAR

  \ Wait for an IRQ to be lowered
  %010 constant IRQ_WAIT

  \ Set PINS
  %000 constant SET_PINS

  \ Set scratch register X (5 LSBs are set to data, all others are cleared)
  %001 constant SET_X

  \ Set scratch register Y (5 LSBs are set to data, all others are cleared)
  %010 constant SET_Y

  \ Set PINDIRS
  %100 constant SET_PINDIRS

  \ PIO JMP instruction
  : jmp, ( address condition -- )
    $07 and 5 lshift swap $1F and or ( $0000 or ) h,
  ;

  \ PIO WAIT instruction
  : wait, ( index source polarity -- )
    0<> 1 and 7 lshift swap 3 and 5 lshift or swap $1F and or $2000 or h,
  ;

  \ PIO IN instruction
  : in, ( bit-count source -- )
    $07 and 5 lshift swap $1F and or $4000 or h,
  ;

  \ PIO OUT instruction
  : out, ( bit-count destination -- )
    $07 and 5 lshift swap $1F and or $6000 or h,
  ;

  \ PIO PUSH instruction
  : push, ( block if-full -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or $8000 or h,
  ;

  \ PIO PULL instruction
  : pull, ( block if-empty -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or $8080 or h,
  ;

  \ PIO MOV instruction
  : mov, ( source op destination -- )
    $07 and 5 lshift swap $03 and 3 lshift or swap $03 and or $A000 or h,
  ;

  \ PIO IRQ instruction
  : irq, ( index set/wait -- )
    $03 and 5 lshift swap $1F and or $C000 or h,
  ;

  \ PIO SET instruction
  : set, ( data destination -- )
    $03 and 5 lshift swap $1F and or $E000 or h,
  ;

  \ PIO JMP instruction with delay or side-set
  : jmp+, ( address delay/side-set condition -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap $1F and or ( $0000 or ) h,
  ;

  \ PIO WAIT instruction with delay or side-set
  : wait+, ( index delay/side-set source polarity -- )
    0<> 1 and 7 lshift swap 3 and 5 lshift or swap $1F and 8 lshift or
    swap $1F and or $2000 or h,
  ;

  \ PIO IN instruction with delay or side-set
  : in+, ( bit-count delay/side-set source -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap $1F and or $4000 or h,
  ;

  \ PIO OUT instruction with delay or side-set
  : out+, ( bit-count delay/side-set destination -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap $1F and or $6000 or h,
  ;

  \ PIO PUSH instruction with delay or side-set
  : push+, ( delay/side-set block if-full -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or swap $1F and 8 lshift or
    $8000 or h,
  ;

  \ PIO PULL instruction with delay or side-set
  : pull+, ( delay/side-set block if-empty -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or swap $1F and 8 lshift or
    $8080 or h,
  ;

  \ PIO MOV instruction with delay or side-set
  : mov+, ( source delay/side-set op destination -- )
    $07 and 5 lshift swap $03 and 3 lshift or swap $1F and 8 lshift or
    swap $03 and or $A000 or h,
  ;

  \ PIO IRQ instruction with delay or side-set
  : irq+, ( index delay/side-set set/wait -- )
    $03 and 5 lshift swap $1F and 8 lshift or swap $1F and or $C000 or h,
  ;

  \ PIO SET instruction with delay or side-set
  : set+, ( data delay/side-set destination -- )
    $03 and 5 lshift swap $1F and 8 lshift or swap $1F and or $E000 or h,
  ;

end-module