;***************************************************************
; Secuencia de LEDs en PORTB - PIC16F877
;***************************************************************

    LIST    P=PIC16F877
    include "P16F877.INC"

    ORG     0x00

;---------------------------------------------------------------
; Configuración
;---------------------------------------------------------------

INICIO
    bcf     STATUS,RP1
    bsf     STATUS,RP0      ; Banco 1
    clrf    TRISB           ; PORTB como salida
    bcf     STATUS,RP0      ; Banco 0

    clrf    PORTB           ; Apaga todos los LEDs

;---------------------------------------------------------------
; Programa principal
;---------------------------------------------------------------

CICLO
    movlw   0x01            ; 00000001
    movwf   PORTB
    call    DELAY

    movlw   0x02            ; 00000010
    movwf   PORTB
    call    DELAY

    movlw   0x04
    movwf   PORTB
    call    DELAY

    movlw   0x08
    movwf   PORTB
    call    DELAY

    movlw   0x10
    movwf   PORTB
    call    DELAY

    movlw   0x20
    movwf   PORTB
    call    DELAY

    movlw   0x40
    movwf   PORTB
    call    DELAY

    movlw   0x80
    movwf   PORTB
    call    DELAY

    goto    CICLO

;---------------------------------------------------------------
; Subrutina de delay
;---------------------------------------------------------------

DELAY
    movlw   d'200'
    movwf   CONT1

LOOP1
    movlw   d'255'
    movwf   CONT2

LOOP2
    decfsz  CONT2,f
    goto    LOOP2

    decfsz  CONT1,f
    goto    LOOP1

    return

;---------------------------------------------------------------
; Variables
;---------------------------------------------------------------

    CBLOCK  0x20
    CONT1
    CONT2
    ENDC

    END