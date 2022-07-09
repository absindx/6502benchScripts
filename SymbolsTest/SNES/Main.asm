;--------------------------------------------------
; 6502bench symbol test
;--------------------------------------------------

;--------------------------------------------------
; ROM setting
;--------------------------------------------------
lorom
arch 65816

;--------------------------------------------------
; Macro
;--------------------------------------------------

macro nextsection(message)
	BRA	?Next
	fillbyte $00
	fill align 16
	fillbyte $2D
	db	"----- <message> -----"
	fill align 32
	fillbyte $00
?Next
endmacro

macro testbank(bank, addr)
;	PEA.w	<bank><<8|<bank>	;\
;	PLB				; | 16bit (not automatically detected)
;	PLB				;/
	LDA	#<bank>
	PHA
	PLB
	LDA	<addr>
	STA	<addr>
?Next
endmacro

;--------------------------------------------------
; Test code
;--------------------------------------------------
	org $008000
EmulationRESET:
		SEI
		CLC
		XCE
		LDA	#$00
		PHA
		PLB

		;--------------------------------------------------
		%nextsection(PPU register)
		;   bank = $00-$3F, $80-$BF
		LDA	$002100		; NG
		LDA	$3F2100		; NG
		LDA	$402100		; NG
		LDA	$7F2100		; NG
		LDA	$802100		; NG
		LDA	$BF2100		; NG
		LDA	$C02100		; NG
		LDA	$FF2100		; NG
		STA	$002100		; OK
		STA	$3F2100		; OK
		STA	$402100		; NG
		STA	$7F2100		; NG
		STA	$802100		; OK
		STA	$BF2100		; OK
		STA	$C02100		; NG
		STA	$FF2100		; NG

		%nextsection(Duplicate named PPU register)
		;OAMDATA         < $2138
		;OAMDATAREAD     < $2138
		;OAMDATA         > $2104
		LDA	$002138		; OK
		STA	$002138		; NG
		LDA	$002104		; NG
		STA	$002104		; OK

		;VMDATAL         < $2139
		;VMDATALREAD     < $2139
		;VMDATAL         > $2118
		LDA	$002139		; OK
		STA	$002139		; NG
		LDA	$002118		; NG
		STA	$002118		; OK

		;VMDATAH         < $213A
		;VMDATAHREAD     < $213A
		;VMDATAH         > $2119
		LDA	$00213A		; OK
		STA	$00213A		; NG
		LDA	$002119		; NG
		STA	$002119		; OK

		;CGDATA          < $213B
		;CGDATAREAD      < $213B
		;CGDATA          > $2122
		LDA	$00213B		; OK
		STA	$00213B		; NG
		LDA	$002122		; NG
		STA	$002122		; OK

		%nextsection(CPU register)
		;   bank = $00-$3F, $80-$BF
		LDA	$004200		; NG
		LDA	$3F4200		; NG
		LDA	$404200		; NG
		LDA	$7F4200		; NG
		LDA	$804200		; NG
		LDA	$BF4200		; NG
		LDA	$C04200		; NG
		LDA	$FF4200		; NG
		STA	$004200		; OK
		STA	$3F4200		; OK
		STA	$404200		; NG
		STA	$7F4200		; NG
		STA	$804200		; OK
		STA	$BF4200		; OK
		STA	$C04200		; NG
		STA	$FF4200		; NG

		%nextsection(DMA register)
		;   bank = $00-$3F, $80-$BF
		LDA	$004300		; OK
		LDA	$3F4300		; OK
		LDA	$404300		; NG
		LDA	$7F4300		; NG
		LDA	$804300		; OK
		LDA	$BF4300		; OK
		LDA	$C04300		; NG
		LDA	$FF4300		; NG
		STA	$004300		; OK
		STA	$3F4300		; OK
		STA	$404300		; NG
		STA	$7F4300		; NG
		STA	$804300		; OK
		STA	$BF4300		; OK
		STA	$C04300		; NG
		STA	$FF4300		; NG

		;--------------------------------------------------
		%nextsection(SA-1 register - write)
		;   bank = $00-$3F, $80-$BF
		LDA	$002200		; NG
		LDA	$3F2200		; NG
		LDA	$402200		; NG
		LDA	$7F2200		; NG
		LDA	$802200		; NG
		LDA	$BF2200		; NG
		LDA	$C02200		; NG
		LDA	$FF2200		; NG
		STA	$002200		; OK
		STA	$3F2200		; OK
		STA	$402200		; NG
		STA	$7F2200		; NG
		STA	$802200		; OK
		STA	$BF2200		; OK
		STA	$C02200		; NG
		STA	$FF2200		; NG

		%nextsection(SA-1 register - read)
		LDA	$002300		; OK
		LDA	$3F2300		; OK
		LDA	$402300		; NG
		LDA	$7F2300		; NG
		LDA	$802300		; OK
		LDA	$BF2300		; OK
		LDA	$C02300		; NG
		LDA	$FF2300		; NG
		STA	$002300		; NG
		STA	$3F2300		; NG
		STA	$402300		; NG
		STA	$7F2300		; NG
		STA	$802300		; NG
		STA	$BF2300		; NG
		STA	$C02300		; NG
		STA	$FF2300		; NG

		;--------------------------------------------------
		%nextsection(Super FX register - write)
		;   bank = $00-$3F, $80-$BF
		LDA	$003000		; OK
		LDA	$3F3000		; OK
		LDA	$403000		; NG
		LDA	$7F3000		; NG
		LDA	$803000		; OK
		LDA	$BF3000		; OK
		LDA	$C03000		; NG
		LDA	$FF3000		; NG
		STA	$003000		; OK
		STA	$3F3000		; OK
		STA	$403000		; NG
		STA	$7F3000		; NG
		STA	$803000		; OK
		STA	$BF3000		; OK
		STA	$C03000		; NG
		STA	$FF3000		; NG

		;--------------------------------------------------
		%nextsection(Bank change test)

		%testbank($00, $2100)
		%testbank($3F, $2100)
		%testbank($40, $2100)
		%testbank($7F, $2100)
		%testbank($80, $2100)
		%testbank($BF, $2100)
		%testbank($C0, $2100)
		%testbank($FF, $2100)



;--------------------------------------------------

	pad $009000
