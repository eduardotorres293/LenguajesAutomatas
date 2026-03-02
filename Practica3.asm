;****************************************************************
;Microcontrolador PIC16F877
;Al iniciar se enciende el RB0
;Al activar RC0 se encendera RB1, RB2 y RB3
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

;****************************************************************
;Limpieza de puertos B y C
;****************************************************************
	movlw	00h
	movwf	PORTB
	movwf	PORTC

;****************************************************************
;Enciende el LED del bit 0 en el puerto B
;****************************************************************
	bsf	PORTB,0


;****************************************************************
;CICLO
;****************************************************************
CICLO
	btfss	PORTC,0
	goto	CICLO		;El bit es cero
	movlw	0Fh			;El bit es uno
	movwf	PORTB
	goto	CICLO

	END