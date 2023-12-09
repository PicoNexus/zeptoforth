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

begin-module bitmap-line-test
  
  oo import
  bitmap import
  ssd1306 import
  bitmap-utils import
  rng import
  
  128 constant my-width
  64 constant my-height
  
  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306
  
  4 constant my-sprite-width
  4 constant my-sprite-height
  
  my-sprite-width my-sprite-height bitmap-buf-size constant my-sprite-buf-size
  my-sprite-buf-size 4 align buffer: my-sprite-buf
  <bitmap> class-size buffer: my-sprite
  
  : run-test ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1 <ssd1306> my-ssd1306 init-object
    my-sprite-buf my-sprite-width my-sprite-height <bitmap> my-sprite init-object
    $FF 1 0 2 1 op-set my-sprite draw-rect-const
    $FF 0 1 4 2 op-set my-sprite draw-rect-const
    $FF 1 3 2 1 op-set my-sprite draw-rect-const
    
    0 { counter }
    
    begin key? not while
      random my-width umod { start-col }
      random my-height umod { start-row }
      random my-width umod { end-col }
      random my-height umod { end-row }
      0 0 4 4 start-col start-row end-col end-row op-or my-sprite my-ssd1306 draw-bitmap-line
      my-ssd1306 update-display
      100 ms
      1 +to counter
      counter 25 umod 0= if
        0 to counter
        my-ssd1306 clear-bitmap
      then
    repeat
    
    key drop
  ;
  
end-module