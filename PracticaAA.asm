;****************************************************************
;Microcontrolador PIC16F877
;Al iniciar se enciende RB0
;Al activar RC0 se enciende RB1
;Al activar RC1 se apagan RB0 y RB1 y se enciende RB2
;****************************************************************

	LIST	P=PIC16F877
	include "P16F877.INC"

	ORG	0X00

;****************************************************************
;Configuracion del puerto B y C
;****************************************************************

INICIO
	bsf	STATUS,RP0

;Puerto B de salida
	movlw	00h		;0utput
	movwf	TRISB

;Puerto C de entrada
	movlw	0FFh	;1nput
	movwf	TRISC

	bcf	STATUS,RP0

	clrf	PORTB

	bsf PORTB,0



;****************************************************************
;CICLO
;****************************************************************
CICLO
;Si RC0 = 1 ejecutar estado 1
PRESIONAR
	btfss	PORTC,0
	goto PRESIONAR

	incf PORTB, 1
SOLTAR
	btfsc	PORTC,0
	goto SOLTAR

	goto	CICLO
	END

CICLO
PRESIONAR
	btfss	PORTC,1
	goto PRESIONAR

	decf PORTB, 1
SOLTAR
	btfsc	PORTC,1
	goto SOLTAR

	goto	CICLO