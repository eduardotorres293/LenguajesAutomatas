LIST P=16F877
        INCLUDE <P16F877.INC>

        __CONFIG _HS_OSC & _WDT_OFF & _PWRTE_OFF & _LVP_OFF

        ORG 0x00
        GOTO INICIO

;---------------------------------
INICIO
        BSF STATUS, RP0      ; Cambiar a Banco 1
        CLRF TRISB           ; PORTB como salida
        BCF STATUS, RP0      ; Regresar a Banco 0
        CLRF PORTB           ; Apagar todos los LEDs

;---------------------------------
LOOP

        ; LED RB5
        MOVLW b'00100000'
        MOVWF PORTB
        CALL DELAY

        ; LED RB6
        MOVLW b'01000000'
        MOVWF PORTB
        CALL DELAY

        ; LED RB7
        MOVLW b'10000000'
        MOVWF PORTB
        CALL DELAY

        ; LED RB3
        MOVLW b'00001000'
        MOVWF PORTB
        CALL DELAY

        GOTO LOOP

;---------------------------------
; Delay
DELAY
        MOVLW   D'200'
        MOVWF   CONT1
DELAY1
        MOVLW   D'250'
        MOVWF   CONT2
DELAY2
        DECFSZ  CONT2, F
        GOTO    DELAY2
        DECFSZ  CONT1, F
        GOTO    DELAY1
        RETURN

;---------------------------------
; Variables
        CBLOCK 0x20
        CONT1
        CONT2
        ENDC

        END