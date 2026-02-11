;****************************************************************
;Encendido de 3 LEDs con microcontrolador PIC16F877
;Activacion de RB5, RB6 y RB7
;****************************************************************

	LIST	P=PIC16F877
	include "P16F877.INC"

	ORG	0X00
;	goto	INICIO
;	ORG	0X05

;****************************************************************
;Configuracion del puerto B
;****************************************************************

INICIO
	bsf	STATUS,RP0
	movlw	00h
	movwf	TRISB
	bcf	STATUS,RP0

;****************************************************************
;Enciende el LED del bit 0 en el puerto B
;****************************************************************
	movlw	00h
	movwf	PORTB
CICLO
;	bsf	PORTB,5
;	bsf	PORTB,6
;	bsf	PORTB,7
	movlw	0E0h
	movwf	PORTB
	goto	CICLO

	END