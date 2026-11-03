0801  0B          ???
0802  08        PHP 
0803  D3          ???
0804  07          ???
0805  9E          ???
0806  32          ???
0807  30 35     BMI 083E
0809  39 00 A0  AND A000,Y
080C  00        BRK 
080D  78        SEI 
080E  E6 01     INC 01
0810  BA        TSX 
0811  BD EE 3D  LDA 3DEE,X
0814  9D FC 00  STA 00FC,X
0817  CA        DEX 
0818  D0 F7     BNE 0811
081A  4C AC 3E  JMP 3EAC
081D  80          ???
081E  00        BRK 
081F  1C          ???
0820  3D 42 FF  AND FF42,X
0823  25 EA     AND EA
0825  80          ???
0826  AA        TAX 
0827  91 02     STA (02),Y
0829  2D 3F E9  AND E93F
082C  15 09     ORA 09,X
082E  31 26     AND (26),Y
0830  44          ???
0831  18        CLC 
0832  A4 C8     LDY C8
0834  2A        ROL A
0835  16 4A     ASL 4A,X
0837  42          ???
0838  FB          ???
0839  A4 B9     LDY B9
083B  10 18     BPL 0855
083D  A9 00     LDA #00
083F  29 01     AND #01
0841  15 D2     ORA D2,X
0843  58        CLI 
0844  83          ???
0845  27          ???
0846  59 0F 8D  EOR 8D0F,Y
0849  F8        SED 
084A  57          ???
084B  07          ???
084C  A2 3F     LDX #3F
084E  E9 83     SBC #83
0850  C0 03     CPY #03
0852  77          ???
0853  5A          ???
0854  FA          ???
0855  AA        TAX 
0856  13          ???
0857  40        RTI 
0858  C6 55     DEC 55
085A  25 F0     AND F0
085C  F4          ???
085D  42          ???
085E  9D 89 03  STA 0389,X
0861  BC 8B B5  LDY B58B,X
0864  DC          ???
0865  6D 87 A2  ADC A287
0868  45 65     EOR 65
086A  F9 8D 12  SBC 128D,Y
086D  D0 AD     BNE 081C
086F  9F          ???
0870  0D DC 58  ORA 58DC
0873  FC          ???
0874  05 D0     ORA D0
0876  BE 52 0A  LDX 0A52,Y
0879  D8        CLD 
087A  A0 D9     LDY #D9
087C  AF          ???
087D  0A        ASL A
087E  45 62     EOR 62
0880  DA          ???
0881  A9 A0     LDA #A0
0883  BA        TSX 
0884  0A        ASL A
0885  04          ???
0886  A0 05     LDY #05
0888  AA        TAX 
0889  2A        ROL A
088A  D7          ???
088B  9D 58 06  STA 0658,X
088E  E8        INX 
088F  D0 E1     BNE 0872
0891  60        RTS 
0892  5F          ???
0893  E0 0A     CPX #0A
0895  1B          ???
0896  40        RTI 
0897  8B          ???
0898  0B          ???
0899  8B          ???
089A  80          ???
089B  0D 37 80  ORA 8037
089E  08        PHP 
089F  6F          ???
08A0  3C          ???
08A1  09 69     ORA #69
08A3  C1 DE     CMP (DE,X)
08A5  29 90     AND #90
08A7  90 CC     BCC 0875
08A9  81 55     STA (55,X)
08AB  29 6A     AND #6A
08AD  0B          ???
08AE  85 C6     STA C6
08B0  0A        ASL A
08B1  60        RTS 
08B2  E6 F9     INC F9
08B4  15 C9     ORA C9,X
08B6  28        PLP 
08B7  CB          ???
08B8  D0 0F     BNE 08C9
08BA  DC          ???
08BB  E3          ???
08BC  07          ???
08BD  A5 64     LDA 64
08BF  90 5D     BCC 091E
08C1  28        PLP 
08C2  85 08     STA 08
08C4  90 7F     BCC 0945
08C6  2A        ROL A
08C7  09 60     ORA #60
08C9  E9 0B     SBC #0B
08CB  69 40     ADC #40
08CD  71 A5     ADC (A5),Y
08CF  AA        TAX 
08D0  A2 99     LDX #99
08D2  F0 11     BEQ 08E5
08D4  77          ???
08D5  A3          ???
08D6  58        CLI 
08D7  E9 11     SBC #11
08D9  7B          ???
08DA  E6 75     INC 75
08DC  4C A2 F2  JMP F2A2
08DF  0C          ???
08E0  39 02 5E  AND 5E02,Y
08E3  36 AC     ROL AC,X
08E5  F0 D5     BEQ 08BC
08E7  56 D0     LSR D0,X
08E9  E7          ???
08EA  75 88     ADC 88,X
08EC  C9 8F     CMP #8F
08EE  D0 05     BNE 08F5
08F0  4F          ???
08F1  96 F0     STX F0,Y
08F3  F3          ???
08F4  1B          ???
08F5  1E 06 1B  ASL 1B06,X
08F8  22          ???
08F9  8C 55 54  STY 5455
08FC  46 8D     LSR 8D
08FE  D0 09     BNE 0909
0900  BD 61 BA  LDA BA61,X
0903  DA          ???
0904  C3          ???
0905  C9 8C     CMP #8C
0907  D0 1F     BNE 0928
0909  BE 80 AA  LDX AA80,Y
090C  49 00     EOR #00
090E  DE 42 34  DEC 3442,X
0911  A4 94     LDY 94
0913  52          ???
0914  48        PHA 
0915  AD 03 68  LDA 6803
0918  CA        DEX 
0919  72          ???
091A  5A          ???
091B  49 5D     EOR #5D
091D  82          ???
091E  A4 07     LDY 07
0920  49 80     EOR #80
0922  91 08     STA (08),Y
0924  7F          ???
0925  18        CLC 
0926  35 B2     AND B2,X
0928  2C D1 C4  BIT C4D1
092B  E9 A4     SBC #A4
092D  82          ???
092E  DA          ???
092F  D2          ???
0930  1B          ???
0931  13          ???
0932  8D 77 02  STA 0277
0935  85 C6     STA C6
0937  5F          ???
0938  20 E4 FF  JSR FFE4
093B  F0 FB     BEQ 0938
093D  C9 53     CMP #53
093F  D0 FF     BNE 0940
0941  06 C8     ASL C8
0943  4C 0D A4  JMP A40D
0946  94 45     STY 45,X
0948  D0 11     BNE 095B
094A  AE 89 CA  LDX CA89
094D  C8        INY 
094E  DA          ???
094F  B1 0A     LDA (0A),Y
0951  20 A2 3D  JSR 3DA2
0954  BE 8E D0  LDX D08E,Y
0957  CB          ???
0958  F7          ???
0959  4C 94 3E  JMP 3E94
095C  FC          ???
095D  2B          ???
095E  5C          ???
095F  13          ???
0960  5C          ???
0961  03          ???
0962  F0 1F     BEQ 0983
0964  C9 2D     CMP #2D
0966  5F          ???
0967  D0 05     BNE 096E
0969  A9 25     LDA #25
096B  31 47     AND (47),Y
096D  CA        DEX 
096E  D5 C6     CMP C6,X
0970  E5 2C     SBC 2C
0972  45 E6     EOR E6
0974  FF          ???
0975  7A          ???
0976  95 A8     STA A8,X
0978  18        CLC 
0979  4C FB 7C  JMP 7CFB
097C  A2 B3     LDX #B3
097E  1E BF 3E  ASL 3EBF,X
0981  20 67 3D  JSR 3D67
0984  BE 7D 37  LDX 377D,Y
0987  53          ???
0988  EA        NOP 
0989  4C E2 FC  JMP FCE2
098C  A0 00     LDY #00
098E  FF          ???
098F  04          ???
0990  2F          ???
0991  0C          ???
0992  A5 2E     LDA 2E
0994  F0 FC     BEQ 0992
0996  A9 97     LDA #97
0998  00        BRK 
0999  81 C6     STA (C6,X)
099B  B5 0C     LDA 0C,X
099D  B2          ???
099E  F4          ???
099F  BD DE 3E  LDA 3EDE,X
09A2  8D 21 FC  STA FC21
09A5  E8        INX 
09A6  C8        INY 
09A7  C0 97     CPY #97
09A9  D0 25     BNE 09D0
09AB  E4 60     CPX 60
09AD  00        BRK 
09AE  FC          ???
09AF  A9 62     LDA #62
09B1  84 03     STY 03
09B3  CB          ???
09B4  A7          ???
09B5  6E 00 A5  ROR A500
09B8  0D DF 7A  ORA 7ADF
09BB  B9 3E 32  LDA 323E,Y
09BE  3F          ???
09BF  C9 30     CMP #30
09C1  D0 02     BNE 09C5
09C3  BF          ???
09C4  A9 82     LDA #82
09C6  50 15     BVC 09DD
09C8  3A          ???
09C9  A0 30     LDY #30
09CB  2A        ROL A
09CC  98        TYA 
09CD  20 47 E8  JSR E847
09D0  B1 1B     LDA (1B),Y
09D2  71 8A     ADC (8A),Y
09D4  8D 63 78  STA 7863
09D7  A9 2B     LDA #2B
09D9  33          ???
09DA  BC A0 55  LDY 55A0,X
09DD  40        RTI 
09DE  C1 76     CMP (76,X)
09E0  AD 00 10  LDA 1000
09E3  97          ???
09E4  9D C2 03  STA 03C2,X
09E7  E8        INX 
09E8  14          ???
09E9  A8        TAY 
09EA  EE 1A 3F  INC 3F1A
09ED  B8        CLV 
09EE  0A        ASL A
09EF  E8        INX 
09F0  E0 18     CPX #18
09F2  D0 E7     BNE 09DB
09F4  FE 11 58  INC 5811,X
09F7  60        RTS 
09F8  A2 00     LDX #00
09FA  C9 DF     CMP #DF
09FC  30 A5     BMI 09A3
09FE  07          ???
09FF  E8        INX 
0A00  38        SEC 
0A01  E9 0A     SBC #0A
0A03  4C 34 3F  JMP 3F34
0A06  FF          ???
0A07  18        CLC 
0A08  E0 A8     CPX #A8
0A0A  8A        TXA 
0A0B  69 30     ADC #30
0A0D  AF          ???
0A0E  60        RTS 
0A0F  48        PHA 
0A10  C0 0A     CPY #0A
0A12  AA        TAX 
0A13  68        PLA 
0A14  57          ???
0A15  00        BRK 
0A16  4A        LSR A
0A17  09 3B     ORA #3B
0A19  D8        CLD 
0A1A  60        RTS 
0A1B  A9 35     LDA #35
0A1D  F5 EE     SBC EE,X
0A1F  95 19     STA 19,X
0A21  D0 C0     BNE 09E3
0A23  0E EA A9  ASL A9EA
0A26  13          ???
0A27  F2          ???
0A28  03          ???
0A29  A2 20     LDX #20
0A2B  CA        DEX 
0A2C  10 FD     BPL 0A2B
0A2E  7C          ???
0A2F  1B          ???
0A30  8D 11 97  STA 9711
0A33  D0 A9     BNE 09DE
0A35  36 F0     ROL F0,X
0A37  01 85     ORA (85,X)
0A39  0E 4C 31  ASL 314C
0A3C  EA        NOP 
0A3D  BF          ???
0A3E  00        BRK 
0A3F  30 00     BMI 0A41
0A41  00        BRK 
0A42  08        PHP 
0A43  22          ???
0A44  1E FC 80  ASL 80FC,X
0A47  52          ???
0A48  21 EF     AND (EF,X)
0A4A  D1 7B     CMP (7B),Y
0A4C  7F          ???
0A4D  29 6F     AND #6F
0A4F  0F          ???
0A50  49 4E     EOR #4E
0A52  D7          ???
0A53  4B          ???
0A54  BB          ???
0A55  EA        NOP 
0A56  CA        DEX 
0A57  DF          ???
0A58  64          ???
0A59  53          ???
0A5A  7F          ???
0A5B  8C 24 40  STY 4024
0A5E  BE 1C 2B  LDX 2B1C,Y
0A61  2F          ???
0A62  7B          ???
0A63  2D 3A CC  AND CC3A
0A66  E3          ???
0A67  16 FA     ASL FA,X
0A69  E8        INX 
0A6A  59 A2 14  EOR 14A2,Y
0A6D  22          ???
0A6E  87          ???
0A6F  75 FE     ADC FE,X
0A71  0E 15 2F  ASL 2F15
0A74  45 B8     EOR B8
0A76  73          ???
0A77  33          ???
0A78  09 33     ORA #33
0A7A  24 54     BIT 54
0A7C  53          ???
0A7D  A9 03     LDA #03
0A7F  54          ???
0A80  49 4D     EOR #4D
0A82  7A          ???
0A83  47          ???
0A84  6A        ROR A
0A85  FF          ???
0A86  4C 61 47  JMP 4761
0A89  51 3B     EOR (3B),Y
0A8B  F6 53     INC 53,X
0A8D  A4 18     LDY 18
0A8F  05 0C     ORA 0C
0A91  46 EF     LSR EF
0A93  41 E1     EOR (E1,X)
0A95  28        PLP 
0A96  F7          ???
0A97  F0 02     BEQ 0A9B
0A99  09 FB     ORA #FB
0A9B  9D DF 31  STA 31DF,X
0A9E  A0 D3     LDY #D3
0AA0  3C          ???
0AA1  0E 62 F5  ASL F562
0AA4  82          ???
0AA5  84 F5     STY F5
0AA7  EE A9 DA  INC DAA9
0AAA  EE 09 11  INC 1109
0AAD  82          ???
0AAE  EB          ???
0AAF  E3          ???
0AB0  A5 C8     LDA C8
0AB2  F4          ???
0AB3  44          ???
0AB4  84 1A     STY 1A
0AB6  79 96 3C  ADC 3C96,Y
0AB9  2B          ???
0ABA  CE 02 F8  DEC F802
0ABD  F7          ???
0ABE  10 3D     BPL 0AFD
0AC0  BE 33 9A  LDX 9A33,Y
0AC3  9F          ???
0AC4  A2 D3     LDX #D3
0AC6  42          ???
0AC7  45 46     EOR 46
0AC9  D4          ???
0ACA  01 97     ORA (97,X)
0ACC  F0 23     BEQ 0AF1
0ACE  0C          ???
0ACF  49 93     EOR #93
0AD1  4F          ???
0AD2  26 C5     ROL C5
0AD4  09 43     ORA #43
0AD6  A2 60     LDX #60
0AD8  BB          ???
0AD9  9E          ???
0ADA  80          ???
0ADB  2A        ROL A
0ADC  43          ???
0ADD  AE 62 1A  LDX 1A62
0AE0  2E 94 BE  ROL BE94
0AE3  DF          ???
0AE4  C2          ???
0AE5  B8        CLV 
0AE6  29 8B     AND #8B
0AE8  3A          ???
0AE9  53          ???
0AEA  3E 45 B5  ROL B545,X
0AED  E8        INX 
0AEE  54          ???
0AEF  97          ???
0AF0  87          ???
0AF1  DA          ???
0AF2  B4 8E     LDY 8E,X
0AF4  A4 6A     LDY 6A
0AF6  39 F6 DE  AND DEF6,Y
0AF9  8D CF 0A  STA 0ACF
0AFC  5A          ???
0AFD  47          ???
0AFE  13          ???
0AFF  0C          ???
0B00  27          ???
0B01  A3          ???
0B02  DE 07 54  DEC 5407,X
0B05  0E 1B F3  ASL F31B
0B08  B6 DF     LDX DF,Y
0B0A  C4 B3     CPY B3
0B0C  35 5D     AND 5D,X
0B0E  C7          ???
0B0F  21 13     AND (13,X)
0B11  D9 53 78  CMP 7853,Y
0B14  20 50 D0  JSR D050
0B17  50 62     BVC 0B7B
0B19  48        PHA 
0B1A  01 A8     ORA (A8,X)
0B1C  D0 2C     BNE 0B4A
0B1E  5D 3B C2  EOR C23B,X
0B21  DF          ???
0B22  22          ???
0B23  1A          ???
0B24  68        PLA 
0B25  11 21     ORA (21),Y
0B27  3A          ???
0B28  3B          ???
0B29  FB          ???
0B2A  B2          ???
0B2B  B5 3F     LDA 3F,X
0B2D  D9 F8 49  CMP 49F8,Y
0B30  EC 0C 53  CPX 530C
0B33  1E F4 29  ASL 29F4,X
0B36  4C 97 22  JMP 2297
0B39  94 D7     STY D7,X
0B3B  0C          ???
0B3C  06 6D     ASL 6D
0B3E  1A          ???
0B3F  3D 27 63  AND 6327,X
0B42  DB          ???
0B43  63          ???
0B44  E6 9E     INC 9E
0B46  8B          ???
0B47  44          ???
0B48  DC          ???
0B49  C6 0B     DEC 0B
0B4B  0B          ???
0B4C  44          ???
0B4D  D9 73 83  CMP 8373,Y
0B50  BC 96 E8  LDY E896,X
0B53  4F          ???
0B54  15 00     ORA 00,X
0B56  53          ???
0B57  45 1F     EOR 1F
0B59  12          ???
0B5A  15 1F     ORA 1F,X
0B5C  D9 04 27  CMP 2704,Y
0B5F  11 DD     ORA (DD),Y
0B61  E5 6C     SBC 6C
0B63  36 75     ROL 75,X
0B65  E2          ???
0B66  08        PHP 
0B67  5D 3A D8  EOR D83A,X
0B6A  F4          ???
0B6B  19 89 EB  ORA EB89,Y
0B6E  30 E3     BMI 0B53
0B70  5C          ???
0B71  C2          ???
0B72  E3          ???
0B73  D0 01     BNE 0B76
0B75  8B          ???
0B76  FA          ???
0B77  8C D6 F2  STY F2D6
0B7A  3C          ???
0B7B  4F          ???
0B7C  8E CC F2  STX F2CC
0B7F  91 29     STA (29),Y
0B81  B2          ???
0B82  16 CF     ASL CF,X
0B84  87          ???
0B85  E8        INX 
0B86  E1 EB     SBC (EB,X)
0B88  9A        TXS 
0B89  83          ???
0B8A  CC 55 17  CPY 1755
0B8D  41 50     EOR (50,X)
0B8F  A6 B3     LDX B3
0B91  07          ???
0B92  A3          ???
0B93  B5 B9     LDA B9,X
0B95  1D 68 FD  ORA FD68,X
0B98  0D 18 AB  ORA AB18
0B9B  76 C5     ROR C5,X
0B9D  11 09     ORA (09),Y
0B9F  AA        TAX 
0BA0  37          ???
0BA1  DA          ???
0BA2  E5 C6     SBC C6
0BA4  97          ???
0BA5  E1 5A     SBC (5A,X)
0BA7  3B          ???
0BA8  66 76     ROR 76
0BAA  0F          ???
0BAB  C0 30     CPY #30
0BAD  A4 8D     LDY 8D
0BAF  1E C1 05  ASL 05C1,X
0BB2  95 60     STA 60,X
0BB4  12          ???
0BB5  6A        ROR A
0BB6  B0 A2     BCS 0B5A
0BB8  D3          ???
0BB9  D8        CLD 
0BBA  42          ???
0BBB  0C          ???
0BBC  7F          ???
0BBD  D2          ???
0BBE  13          ???
0BBF  7B          ???
0BC0  08        PHP 
0BC1  1E 07 8B  ASL 8B07,X
0BC4  95 70     STA 70,X
0BC6  A4 62     LDY 62
0BC8  23          ???
0BC9  7D 0F AE  ADC AE0F,X
0BCC  83          ???
0BCD  ED D4 26  SBC 26D4
0BD0  B6 FC     LDX FC,Y
0BD2  19 96 5B  ORA 5B96,Y
0BD5  E0 AB     CPX #AB
0BD7  56 36     LSR 36,X
0BD9  35 C0     AND C0,X
0BDB  B4 2A     LDY 2A,X
0BDD  06 12     ASL 12
0BDF  17          ???
0BE0  67          ???
0BE1  C5 C8     CMP C8
0BE3  DC          ???
0BE4  0E A5 F1  ASL F1A5
0BE7  DE CA 77  DEC 77CA,X
0BEA  06 A7     ASL A7
0BEC  06 4E     ASL 4E
0BEE  E5 EA     SBC EA
0BF0  E4 F6     CPX F6
0BF2  46 51     LSR 51
0BF4  FC          ???
0BF5  89          ???
0BF6  72          ???
0BF7  D8        CLD 
0BF8  3E 0B 09  ROL 090B,X
0BFB  B7          ???
0BFC  A0 5F     LDY #5F
0BFE  A1 6A     LDA (6A,X)
0C00  10 37     BPL 0C39
0C02  FE 42 54  INC 5442,X
0C05  18        CLC 
0C06  E4 CF     CPX CF
0C08  4F          ???
0C09  E1 8A     SBC (8A,X)
0C0B  3C          ???
0C0C  0D AD 55  ORA 55AD
0C0F  F0 5F     BEQ 0C70
0C11  8F          ???
0C12  C1 45     CMP (45,X)
0C14  22          ???
0C15  C3          ???
0C16  B8        CLV 
0C17  99 12 76  STA 7612,Y
0C1A  B0 D7     BCS 0BF3
0C1C  CD 8C 94  CMP 948C
0C1F  03          ???
0C20  F4          ???
0C21  B8        CLV 
0C22  76 EB     ROR EB,X
0C24  10 67     BPL 0C8D
0C26  00        BRK 
0C27  F4          ???
0C28  78        SEI 
0C29  EC 70 BB  CPX BB70
0C2C  0D BD 03  ORA 03BD
0C2F  91 4D     STA (4D),Y
0C31  EC 3A F3  CPX F33A
0C34  8E 91 65  STX 6591
0C37  6A        ROR A
0C38  1C          ???
0C39  57          ???
0C3A  00        BRK 
0C3B  48        PHA 
0C3C  B4 95     LDY 95,X
0C3E  19 21 50  ORA 5021,Y
0C41  D3          ???
0C42  CD 4D 9E  CMP 9E4D
0C45  6C 6F 91  JMP (916F)
0C48  F8        SED 
0C49  13          ???
0C4A  34          ???
0C4B  BA        TSX 
0C4C  EE 25 C5  INC C525
0C4F  41 68     EOR (68,X)
0C51  F5 66     SBC 66,X
0C53  79 A4 0E  ADC 0EA4,Y
0C56  93          ???
0C57  2A        ROL A
0C58  D3          ???
0C59  94 1D     STY 1D,X
0C5B  26 BD     ROL BD
0C5D  A4 8D     LDY 8D
0C5F  02          ???
0C60  A9 E8     LDA #E8
0C62  5D 50 36  EOR 3650,X
0C65  1E 54 2E  ASL 2E54,X
0C68  12          ???
0C69  AC 0D BE  LDY BE0D
0C6C  A0 2D     LDY #2D
0C6E  CC 49 06  CPY 0649
0C71  D1 38     CMP (38),Y
0C73  DE 00 34  DEC 3400,X
0C76  85 21     STA 21
0C78  83          ???
0C79  9A        TXS 
0C7A  7C          ???
0C7B  AC 66 AA  LDY AA66
0C7E  EF          ???
0C7F  D0 31     BNE 0CB2
0C81  53          ???
0C82  48        PHA 
0C83  BE D6 AE  LDX AED6,Y
0C86  E7          ???
0C87  19 B8 32  ORA 32B8,Y
0C8A  BA        TSX 
0C8B  2E 57 80  ROL 8057
0C8E  12          ???
0C8F  3F          ???
0C90  8D 30 4A  STA 4A30
0C93  78        SEI 
0C94  98        TYA 
0C95  AC 1A A6  LDY A61A
0C98  C5 56     CMP 56
0C9A  83          ???
0C9B  70 E1     BVS 0C7E
0C9D  93          ???
0C9E  2C 15 E3  BIT E315
0CA1  5B          ???
0CA2  A8        TAY 
0CA3  BB          ???
0CA4  64          ???
0CA5  18        CLC 
0CA6  EE E0 8D  INC 8DE0
0CA9  25 B1     AND B1
0CAB  7C          ???
0CAC  22          ???
0CAD  47          ???
0CAE  89          ???
0CAF  3A          ???
0CB0  F8        SED 
0CB1  ED 77 E0  SBC E077
0CB4  4C BB B0  JMP B0BB
0CB7  33          ???
0CB8  A8        TAY 
0CB9  3A          ???
0CBA  48        PHA 
0CBB  8A        TXA 
0CBC  AF          ???
0CBD  CB          ???
0CBE  30 8D     BMI 0C4D
0CC0  0C          ???
0CC1  80          ???
0CC2  7F          ???
0CC3  07          ???
0CC4  8E 40 FC  STX FC40
0CC7  77          ???
0CC8  65 EA     ADC EA
0CCA  01 93     ORA (93,X)
0CCC  F2          ???
0CCD  8A        TXA 
0CCE  9D 78 01  STA 0178,X
0CD1  67          ???
0CD2  C0 AD     CPY #AD
0CD4  0F          ???
0CD5  DF          ???
0CD6  46 7C     LSR 7C
0CD8  0D 5C AC  ORA AC5C
0CDB  2C 83 B2  BIT B283
0CDE  64          ???
0CDF  25 99     AND 99
0CE1  E0 5D     CPX #5D
0CE3  CF          ???
0CE4  42          ???
0CE5  CD 1F 83  CMP 831F
0CE8  63          ???
0CE9  9A        TXS 
0CEA  06 24     ASL 24
0CEC  36 1A     ROL 1A,X
0CEE  FC          ???
0CEF  85 C5     STA C5
0CF1  13          ???
0CF2  94 65     STY 65,X
0CF4  70 3A     BVS 0D30
0CF6  62          ???
0CF7  27          ???
0CF8  12          ???
0CF9  19 D5 B0  ORA B0D5,Y
0CFC  C1 0E     CMP (0E,X)
0CFE  0C          ???
0CFF  D3          ???
0D00  05 50     ORA 50
0D02  D5 3F     CMP 3F,X
0D04  48        PHA 
0D05  A7          ???
0D06  D9 83 D5  CMP D583,Y
0D09  B0 15     BCS 0D20
0D0B  5B          ???
0D0C  1E 01 C7  ASL C701,X
0D0F  C3          ???
0D10  44          ???
0D11  96 B1     STX B1,Y
0D13  E6 EF     INC EF
0D15  3A          ???
0D16  55 E0     EOR E0,X
0D18  8D C5 C4  STA C4C5
0D1B  02          ???
0D1C  3E 4A E7  ROL E74A,X
0D1F  3B          ???
0D20  B1 30     LDA (30),Y
0D22  88        DEY 
0D23  9A        TXS 
0D24  83          ???
0D25  4F          ???
0D26  45 C3     EOR C3
0D28  BC 2F 50  LDY 502F,X
0D2B  F4          ???
0D2C  38        SEC 
0D2D  7B          ???
0D2E  F3          ???
0D2F  B4 C3     LDY C3,X
0D31  A9 5E     LDA #5E
0D33  02          ???
0D34  29 4F     AND #4F
0D36  D8        CLD 
0D37  92          ???
0D38  16 95     ASL 95,X
0D3A  27          ???
0D3B  C5 E7     CMP E7
0D3D  71 12     ADC (12),Y
0D3F  3C          ???
0D40  8D EE 5B  STA 5BEE
0D43  FC          ???
0D44  36 07     ROL 07,X
0D46  E0 87     CPX #87
0D48  F7          ???
0D49  C1 08     CMP (08,X)
0D4B  39 0E 68  AND 680E,Y
0D4E  18        CLC 
0D4F  37          ???
0D50  82          ???
0D51  33          ???
0D52  0F          ???
0D53  A8        TAY 
0D54  FA          ???
0D55  6B          ???
0D56  9B          ???
0D57  E1 3B     SBC (3B,X)
0D59  2E 12 73  ROL 7312
0D5C  8A        TXA 
0D5D  53          ???
0D5E  8D 34 9A  STA 9A34
0D61  5E 2C 11  LSR 112C,X
0D64  4B          ???
0D65  60        RTS 
0D66  02          ???
0D67  55 E8     EOR E8,X
0D69  16 0F     ASL 0F,X
0D6B  D4          ???
0D6C  8C BD A2  STY A2BD
0D6F  36 7B     ROL 7B,X
0D71  E4 8A     CPX 8A
0D73  9A        TXS 
0D74  86 DB     STX DB
0D76  62          ???
0D77  24 5F     BIT 5F
0D79  0C          ???
0D7A  2A        ROL A
0D7B  72          ???
0D7C  54          ???
0D7D  10 32     BPL 0DB1
0D7F  17          ???
0D80  B2          ???
0D81  E1 44     SBC (44,X)
0D83  DE 1E 2A  DEC 2A1E,X
0D86  8D DE 3B  STA 3BDE
0D89  89          ???
0D8A  29 0C     AND #0C
0D8C  19 46 87  ORA 8746,Y
0D8F  B1 70     LDA (70),Y
0D91  31 15     AND (15),Y
0D93  37          ???
0D94  1D 96 C6  ORA C696,X
0D97  08        PHP 
0D98  A1 10     LDA (10,X)
0D9A  E6 89     INC 89
0D9C  2C C9 11  BIT 11C9
0D9F  22          ???
0DA0  58        CLI 
0DA1  8E BD BD  STX BDBD
0DA4  AD 93 44  LDA 4493
0DA7  0F          ???
0DA8  60        RTS 
0DA9  2B          ???
0DAA  77          ???
0DAB  00        BRK 
0DAC  8D B7 C1  STA C1B7
0DAF  E8        INX 
0DB0  2B          ???
0DB1  CB          ???
0DB2  E0 08     CPX #08
0DB4  BE 8F 9A  LDX 9A8F,Y
0DB7  7F          ???
0DB8  B6 62     LDX 62,Y
0DBA  C1 E9     CMP (E9,X)
0DBC  48        PHA 
0DBD  EE A2 F4  INC F4A2
0DC0  18        CLC 
0DC1  46 8C     LSR 8C
0DC3  44          ???
0DC4  02          ???
0DC5  68        PLA 
0DC6  93          ???
0DC7  8C 29 51  STY 5129
0DCA  9D AF 48  STA 48AF,X
0DCD  BF          ???
0DCE  A9 F6     LDA #F6
0DD0  82          ???
0DD1  C0 F4     CPY #F4
0DD3  C8        INY 
0DD4  33          ???
0DD5  62          ???
0DD6  06 08     ASL 08
0DD8  6D 0B A6  ADC A60B
0DDB  A0 CC     LDY #CC
0DDD  28        PLP 
0DDE  31 33     AND (33),Y
0DE0  8D A2 8A  STA 8AA2
0DE3  0F          ???
0DE4  E1 1D     SBC (1D,X)
0DE6  9E          ???
0DE7  0C          ???
0DE8  D0 3F     BNE 0E29
0DEA  00        BRK 
0DEB  4C 5A 46  JMP 465A
0DEE  49 30     EOR #30
0DF0  97          ???
0DF1  38        SEC 
0DF2  D3          ???
0DF3  F1 D6     SBC (D6),Y
0DF5  07          ???
0DF6  0C          ???
0DF7  F9 E8 15  SBC 15E8,Y
0DFA  0A        ASL A
0DFB  28        PLP 
0DFC  6C 13 81  JMP (8113)
0DFF  CE 10 2E  DEC 2E10
0E02  83          ???
0E03  F9 66 D5  SBC D566,Y
0E06  71 88     ADC (88),Y
0E08  04          ???
0E09  89          ???
0E0A  C4 7E     CPY 7E
0E0C  D7          ???
0E0D  02          ???
0E0E  F3          ???
0E0F  A0 30     LDY #30
0E11  90 4A     BCC 0E5D
0E13  E0 AA     CPX #AA
0E15  64          ???
0E16  62          ???
0E17  51 C6     EOR (C6),Y
0E19  60        RTS 
0E1A  A6 03     LDX 03
0E1C  E0 F8     CPX #F8
0E1E  42          ???
0E1F  4D EE F0  EOR F0EE
0E22  0A        ASL A
0E23  B5 5A     LDA 5A,X
0E25  02          ???
0E26  49 68     EOR #68
0E28  42          ???
0E29  0E 18 7B  ASL 7B18
0E2C  F9 29 C9  SBC C929,Y
0E2F  2C 80 6B  BIT 6B80
0E32  FB          ???
0E33  E8        INX 
0E34  A9 2D     LDA #2D
0E36  00        BRK 
0E37  03          ???
0E38  78        SEI 
0E39  5A          ???
0E3A  F8        SED 
0E3B  6A        ROR A
0E3C  CC 52 1D  CPY 1D52
0E3F  B8        CLV 
0E40  E8        INX 
0E41  85 88     STA 88
0E43  18        CLC 
0E44  25 E0     AND E0
0E46  7E C8 40  ROR 40C8,X
0E49  CC 59 4B  CPY 4B59
0E4C  32          ???
0E4D  82          ???
0E4E  E1 66     SBC (66,X)
0E50  58        CLI 
0E51  71 CD     ADC (CD),Y
0E53  98        TYA 
0E54  A4 13     LDY 13
0E56  13          ???
0E57  1C          ???
0E58  7F          ???
0E59  81 EF     STA (EF,X)
0E5B  3A          ???
0E5C  6C 98 0E  JMP (0E98)
0E5F  2C 21 E0  BIT E021
0E62  F2          ???
0E63  7E 0B 82  ROR 820B,X
0E66  B3          ???
0E67  70 C4     BVS 0E2D
0E69  AC AE 8F  LDY 8FAE
0E6C  F0 4D     BEQ 0EBB
0E6E  AF          ???
0E6F  C8        INY 
0E70  05 70     ORA 70
0E72  7C          ???
0E73  58        CLI 
0E74  07          ???
0E75  3B          ???
0E76  13          ???
0E77  E6 12     INC 12
0E79  9F          ???
0E7A  3B          ???
0E7B  E5 B2     SBC B2
0E7D  27          ???
0E7E  1A          ???
0E7F  2D C9 0C  AND 0CC9
0E82  1A          ???
0E83  26 0D     ROL 0D
0E85  1F          ???
0E86  10 06     BPL 0E8E
0E88  69 04     ADC #04
0E8A  D3          ???
0E8B  86 B9     STX B9
0E8D  16 5C     ASL 5C,X
0E8F  84 D5     STY D5
0E91  8E 0B 06  STX 060B
0E94  A1 98     LDA (98,X)
0E96  B6 35     LDX 35,Y
0E98  B4 2F     LDY 2F,X
0E9A  FB          ???
0E9B  0D 35 D5  ORA D535
0E9E  B9 E7 A8  LDA A8E7,Y
0EA1  84 86     STY 86
0EA3  E7          ???
0EA4  02          ???
0EA5  6F          ???
0EA6  6A        ROR A
0EA7  F8        SED 
0EA8  77          ???
0EA9  FB          ???
0EAA  EF          ???
0EAB  1F          ???
0EAC  04          ???
0EAD  4D 71 27  EOR 2771
0EB0  00        BRK 
0EB1  56 D0     LSR D0,X
0EB3  87          ???
0EB4  C2          ???
0EB5  13          ???
0EB6  77          ???
0EB7  62          ???
0EB8  5C          ???
0EB9  A7          ???
0EBA  30 F0     BMI 0EAC
0EBC  F6 60     INC 60,X
0EBE  06 E7     ASL E7
0EC0  46 D5     LSR D5
0EC2  47          ???
0EC3  20 2C 0A  JSR 0A2C
0EC6  13          ???
0EC7  B8        CLV 
0EC8  00        BRK 
0EC9  24 E4     BIT E4
0ECB  3B          ???
0ECC  3D A0 84  AND 84A0,X
0ECF  A4 DC     LDY DC
0ED1  0C          ???
0ED2  EB          ???
0ED3  79 BC B9  ADC B9BC,Y
0ED6  77          ???
0ED7  1B          ???
0ED8  41 19     EOR (19,X)
0EDA  E2          ???
0EDB  9A        TXS 
0EDC  26 21     ROL 21
0EDE  10 DD     BPL 0EBD
0EE0  97          ???
0EE1  36 83     ROL 83,X
0EE3  A8        TAY 
0EE4  7E AF 83  ROR 83AF,X
0EE7  BA        TSX 
0EE8  3C          ???
0EE9  44          ???
0EEA  40        RTI 
0EEB  E2          ???
0EEC  7C          ???
0EED  E8        INX 
0EEE  4F          ???
0EEF  C2          ???
0EF0  CC 06 4F  CPY 4F06
0EF3  7E 7B F9  ROR F97B,X
0EF6  1D 07 3A  ORA 3A07,X
0EF9  41 64     EOR (64,X)
0EFB  E4 74     CPX 74
0EFD  49 CA     EOR #CA
0EFF  18        CLC 
0F00  66 D7     ROR D7
0F02  54          ???
0F03  9F          ???
0F04  86 20     STX 20
0F06  D8        CLD 
0F07  36 E4     ROL E4,X
0F09  9F          ???
0F0A  A8        TAY 
0F0B  04          ???
0F0C  2F          ???
0F0D  87          ???
0F0E  33          ???
0F0F  11 A5     ORA (A5),Y
0F11  14          ???
0F12  06 1A     ASL 1A
0F14  15 04     ORA 04,X
0F16  5D CD 32  EOR 32CD,X
0F19  20 DA 55  JSR 55DA
0F1C  CD 5E 81  CMP 815E
0F1F  89          ???
0F20  39 44 99  AND 9944,Y
0F23  12          ???
0F24  29 A8     AND #A8
0F26  8D F9 14  STA 14F9
0F29  38        SEC 
0F2A  1F          ???
0F2B  E2          ???
0F2C  05 18     ORA 18
0F2E  6C B7 46  JMP (46B7)
0F31  84 52     STY 52
0F33  20 16 BE  JSR BE16
0F36  FE 11 3B  INC 3B11,X
0F39  18        CLC 
0F3A  DE 1B 5D  DEC 5D1B,X
0F3D  F8        SED 
0F3E  99 18 62  STA 6218,Y
0F41  8F          ???
0F42  C0 3C     CPY #3C
0F44  1C          ???
0F45  45 4C     EOR 4C
0F47  8D 4A FD  STA FD4A
0F4A  CA        DEX 
0F4B  59 CB 09  EOR 09CB,Y
0F4E  A7          ???
0F4F  49 20     EOR #20
0F51  63          ???
0F52  B5 80     LDA 80,X
0F54  4F          ???
0F55  E3          ???
0F56  04          ???
0F57  7D E4 CE  ADC CEE4,X
0F5A  9E          ???
0F5B  A7          ???
0F5C  26 7A     ROL 7A
0F5E  19 30 FB  ORA FB30,Y
0F61  DB          ???
0F62  B3          ???
0F63  B6 7D     LDX 7D,Y
0F65  C1 30     CMP (30,X)
0F67  2B          ???
0F68  27          ???
0F69  7A          ???
0F6A  E2          ???
0F6B  4E 19 12  LSR 1219
0F6E  00        BRK 
0F6F  27          ???
0F70  B9 B9 FC  LDA FCB9,Y
0F73  41 D2     EOR (D2,X)
0F75  10 D7     BPL 0F4E
0F77  C3          ???
0F78  43          ???
0F79  0D AD 11  ORA 11AD
0F7C  AC 7C 31  LDY 317C
0F7F  7A          ???
0F80  A9 61     LDA #61
0F82  EE 33 25  INC 2533
0F85  62          ???
0F86  40        RTI 
0F87  7E E9 13  ROR 13E9,X
0F8A  B2          ???
0F8B  44          ???
0F8C  59 76 41  EOR 4176,Y
0F8F  E3          ???
0F90  A1 A5     LDA (A5,X)
0F92  1F          ???
0F93  80          ???
0F94  8D B0 1D  STA 1DB0
0F97  E8        INX 
0F98  02          ???
0F99  45 F0     EOR F0
0F9B  1F          ???
0F9C  D9 84 09  CMP 0984,Y
0F9F  F0 03     BEQ 0FA4
0FA1  DD 9F BE  CMP BE9F,X
0FA4  F0 85     BEQ 0F2B
0FA6  FE 4E 84  INC 844E,X
0FA9  6E 2D 6B  ROR 6B2D
0FAC  E9 CC     SBC #CC
0FAE  27          ???
0FAF  8D 59 64  STA 6459
0FB2  D1 F8     CMP (F8),Y
0FB4  B6 EF     LDX EF,Y
0FB6  08        PHP 
0FB7  B9 D3 48  LDA 48D3,Y
0FBA  CC 22 48  CPY 4822
0FBD  10 6D     BPL 102C
0FBF  86 CA     STX CA
0FC1  61 17     ADC (17,X)
0FC3  07          ???
0FC4  7E 83 98  ROR 9883,X
0FC7  3F          ???
0FC8  A4 94     LDY 94
0FCA  7F          ???
0FCB  3F          ???
0FCC  05 36     ORA 36
0FCE  08        PHP 
0FCF  41 F3     EOR (F3,X)
0FD1  27          ???
0FD2  CA        DEX 
0FD3  37          ???
0FD4  20 F4 22  JSR 22F4
0FD7  D6 DF     DEC DF,X
0FD9  09 AA     ORA #AA
0FDB  7A          ???
0FDC  0A        ASL A
0FDD  96 E0     STX E0,Y
0FDF  53          ???
0FE0  06 59     ASL 59
0FE2  B8        CLV 
0FE3  77          ???
0FE4  F2          ???
0FE5  0E 7C B5  ASL B57C
0FE8  EC 1A 76  CPX 761A
0FEB  07          ???
0FEC  47          ???
0FED  98        TYA 
0FEE  BF          ???
0FEF  8A        TXA 
0FF0  F3          ???
0FF1  87          ???
0FF2  C6 76     DEC 76
0FF4  F8        SED 
0FF5  8A        TXA 
0FF6  62          ???
0FF7  C3          ???
0FF8  A4 0C     LDY 0C
0FFA  19 E8 A1  ORA A1E8,Y
0FFD  F0 1E     BEQ 101D
0FFF  F9 F1 2C  SBC 2CF1,Y
1002  1C          ???
1003  2F          ???
1004  A7          ???
1005  07          ???
1006  A0 76     LDY #76
1008  B0 70     BCS 107A
100A  9D B5 CE  STA CEB5,X
100D  68        PLA 
100E  FA          ???
100F  64          ???
1010  9D D9 A8  STA A8D9,X
1013  62          ???
1014  4C 90 A0  JMP A090
1017  7D 86 0A  ADC 0A86,X
101A  CD E1 90  CMP 90E1
101D  9D 92 8D  STA 8D92,X
1020  F7          ???
1021  C0 60     CPY #60
1023  BA        TSX 
1024  DF          ???
1025  C0 07     CPY #07
1027  A8        TAY 
1028  93          ???
1029  5B          ???
102A  C7          ???
102B  17          ???
102C  45 88     EOR 88
102E  81 E1     STA (E1,X)
1030  BE EA 2A  LDX 2AEA,Y
1033  1F          ???
1034  C0 15     CPY #15
1036  62          ???
1037  F0 05     BEQ 103E
1039  0F          ???
103A  2D CA 2C  AND 2CCA
103D  A8        TAY 
103E  CB          ???
103F  F3          ???
1040  2A        ROL A
1041  D8        CLD 
1042  4E 0B 07  LSR 070B
1045  C7          ???
1046  10 E3     BPL 102B
1048  F8        SED 
1049  09 5C     ORA #5C
104B  5B          ???
104C  1C          ???
104D  2C 47 04  BIT 0447
1050  D5 3B     CMP 3B,X
1052  10 D3     BPL 1027
1054  9F          ???
1055  3D 74 73  AND 7374,X
1058  F0 FC     BEQ 1056
105A  6A        ROR A
105B  55 2B     EOR 2B,X
105D  D4          ???
105E  F2          ???
105F  A0 AB     LDY #AB
1061  36 27     ROL 27,X
1063  D3          ???
1064  04          ???
1065  15 EE     ORA EE,X
1067  C0 41     CPY #41
1069  D9 CC 1A  CMP 1ACC,Y
106C  91 C7     STA (C7),Y
106E  AB          ???
106F  12          ???
1070  8C 3A 78  STY 783A
1073  F9 AC 3A  SBC 3AAC,Y
1076  ED 1B 33  SBC 331B
1079  08        PHP 
107A  83          ???
107B  C7          ???
107C  F2          ???
107D  64          ???
107E  D9 07 71  CMP 7107,Y
1081  97          ???
1082  73          ???
1083  0A        ASL A
1084  0E 69 C5  ASL C569
1087  78        SEI 
1088  5A          ???
1089  31 1D     AND (1D),Y
108B  4F          ???
108C  81 22     STA (22,X)
108E  E4 13     CPX 13
1090  BA        TSX 
1091  63          ???
1092  10 AB     BPL 103F
1094  7F          ???
1095  62          ???
1096  38        SEC 
1097  69 C4     ADC #C4
1099  B5 27     LDA 27,X
109B  19 5A 25  ORA 255A,Y
109E  68        PLA 
109F  6F          ???
10A0  D9 E8 0B  CMP 0BE8,Y
10A3  43          ???
10A4  13          ???
10A5  02          ???
10A6  96 E4     STX E4,Y
10A8  77          ???
10A9  BF          ???
10AA  BD 9D CA  LDA CA9D,X
10AD  39 1F EE  AND EE1F,Y
10B0  F0 24     BEQ 10D6
10B2  63          ???
10B3  2E 20 9F  ROL 9F20
10B6  A8        TAY 
10B7  44          ???
10B8  EC 30 66  CPX 6630
10BB  56 16     LSR 16,X
10BD  BF          ???
10BE  79 E4 E6  ADC E6E4,Y
10C1  5F          ???
10C2  95 89     STA 89,X
10C4  7C          ???
10C5  FC          ???
10C6  31 95     AND (95),Y
10C8  E9 E3     SBC #E3
10CA  99 08 0A  STA 0A08,Y
10CD  10 E8     BPL 10B7
10CF  EA        NOP 
10D0  52          ???
10D1  85 E1     STA E1
10D3  48        PHA 
10D4  28        PLP 
10D5  84 C2     STY C2
10D7  1A          ???
10D8  F0 3F     BEQ 1119
10DA  99 53 30  STA 3053,Y
10DD  E4 8C     CPX 8C
10DF  E4 D0     CPX D0
10E1  9E          ???
10E2  CC 05 7A  CPY 7A05
10E5  33          ???
10E6  8C A1 B3  STY B3A1
10E9  CC D1 68  CPY 68D1
10EC  15 A4     ORA A4,X
10EE  AC 7F B8  LDY B87F
10F1  4E F4 D7  LSR D7F4
10F4  76 8D     ROR 8D,X
10F6  33          ???
10F7  2A        ROL A
10F8  F8        SED 
10F9  3B          ???
10FA  57          ???
10FB  0C          ???
10FC  B9 FF 2E  LDA 2EFF,Y
10FF  84 E4     STY E4
1101  9C          ???
1102  78        SEI 
1103  E9 1F     SBC #1F
1105  12          ???
1106  BF          ???
1107  5E 32 98  LSR 9832,X
110A  8E 0B F8  STX F80B
110D  F1 3A     SBC (3A),Y
110F  AC 2A A1  LDY A12A
1112  06 F9     ASL F9
1114  98        TYA 
1115  17          ???
1116  39 72 0F  AND 0F72,Y
1119  92          ???
111A  88        DEY 
111B  F1 AB     SBC (AB),Y
111D  28        PLP 
111E  9D 5A E9  STA E95A,X
1121  2D C1 8D  AND 8DC1
1124  C8        INY 
1125  05 AC     ORA AC
1127  D9 BE 5C  CMP 5CBE,Y
112A  5B          ???
112B  8E 39 C7  STX C739
112E  5F          ???
112F  3A          ???
1130  0F          ???
1131  63          ???
1132  42          ???
1133  F8        SED 
1134  5C          ???
1135  95 31     STA 31,X
1137  6A        ROR A
1138  FA          ???
1139  34          ???
113A  B5 ED     LDA ED,X
113C  6B          ???
113D  6C A3 86  JMP (86A3)
1140  7C          ???
1141  00        BRK 
1142  A4 E9     LDY E9
1144  47          ???
1145  86 66     STX 66
1147  41 41     EOR (41,X)
1149  2F          ???
114A  2B          ???
114B  5C          ???
114C  98        TYA 
114D  8C 38 3D  STY 3D38
1150  B7          ???
1151  6F          ???
1152  B1 76     LDA (76),Y
1154  0D 11 E0  ORA E011
1157  67          ???
1158  74          ???
1159  7F          ???
115A  BE 51 31  LDX 3151,Y
115D  60        RTS 
115E  2D 0C 91  AND 910C
1161  1E A4 E4  ASL E4A4,X
1164  6D 7B 9F  ADC 9F7B
1167  75 E2     ADC E2,X
1169  74          ???
116A  C9 33     CMP #33
116C  39 34 88  AND 8834,Y
116F  CF          ???
1170  7B          ???
1171  15 6C     ORA 6C,X
1173  86 33     STX 33
1175  8E 04 D2  STX D204
1178  94 CB     STY CB,X
117A  E2          ???
117B  5C          ???
117C  5D 2B 1C  EOR 1C2B,X
117F  F4          ???
1180  E4 08     CPX 08
1182  68        PLA 
1183  39 27 DB  AND DB27,Y
1186  A5 39     LDA 39
1188  0C          ???
1189  BB          ???
118A  49 68     EOR #68
118C  73          ???
118D  3B          ???
118E  B8        CLV 
118F  12          ???
1190  F1 6A     SBC (6A),Y
1192  46 74     LSR 74
1194  32          ???
1195  3F          ???
1196  DC          ???
1197  E1 A7     SBC (A7,X)
1199  25 EA     AND EA
119B  C4 AC     CPY AC
119D  48        PHA 
119E  F4          ???
119F  2E 9C AA  ROL AA9C
11A2  D3          ???
11A3  4F          ???
11A4  DC          ???
11A5  B6 17     LDX 17,Y
11A7  E0 30     CPX #30
11A9  87          ???
11AA  0D EB 20  ORA 20EB
11AD  52          ???
11AE  69 CF     ADC #CF
11B0  13          ???
11B1  15 4F     ORA 4F,X
11B3  75 28     ADC 28,X
11B5  85 AB     STA AB
11B7  82          ???
11B8  7D 08 49  ADC 4908,X
11BB  6E E1 70  ROR 70E1
11BE  E1 4B     SBC (4B,X)
11C0  4C 87 44  JMP 4487
11C3  57          ???
11C4  51 10     EOR (10),Y
11C6  63          ???
11C7  3B          ???
11C8  05 5D     ORA 5D
11CA  35 4D     AND 4D,X
11CC  18        CLC 
11CD  94 69     STY 69,X
11CF  63          ???
11D0  D4          ???
11D1  27          ???
11D2  77          ???
11D3  2A        ROL A
11D4  97          ???
11D5  3B          ???
11D6  BD F9 95  LDA 95F9,X
11D9  42          ???
11DA  B5 1E     LDA 1E,X
11DC  28        PLP 
11DD  96 73     STX 73,Y
11DF  35 DF     AND DF,X
11E1  15 C4     ORA C4,X
11E3  F8        SED 
11E4  12          ???
11E5  AC 01 0C  LDY 0C01
11E8  63          ???
11E9  E4 EE     CPX EE
11EB  20 4D D0  JSR D04D
11EE  83          ???
11EF  AF          ???
11F0  8C 74 A2  STY A274
11F3  13          ???
11F4  3D AD 94  AND 94AD,X
11F7  3F          ???
11F8  2C 52 47  BIT 4752
11FB  09 85     ORA #85
11FD  3C          ???
11FE  CC 2A EE  CPY EE2A
1201  B8        CLV 
1202  B3          ???
1203  FA          ???
1204  F6 1F     INC 1F,X
1206  C9 C7     CMP #C7
1208  61 ED     ADC (ED,X)
120A  A0 3B     LDY #3B
120C  8D 46 C8  STA C846
120F  D0 AA     BNE 11BB
1211  1C          ???
1212  32          ???
1213  B9 05 5D  LDA 5D05,Y
1216  F6 ED     INC ED,X
1218  67          ???
1219  15 91     ORA 91,X
121B  FC          ???
121C  27          ???
121D  A3          ???
121E  2A        ROL A
121F  E4 9D     CPX 9D
1221  2E 8A F6  ROL F68A
1224  32          ???
1225  53          ???
1226  03          ???
1227  8A        TXA 
1228  D5 7D     CMP 7D,X
122A  2A        ROL A
122B  CC B3 7A  CPY 7AB3
122E  0A        ASL A
122F  84 FB     STY FB
1231  40        RTI 
1232  5A          ???
1233  BC 9C 06  LDY 069C,X
1236  A0 2D     LDY #2D
1238  21 89     AND (89,X)
123A  EF          ???
123B  7C          ???
123C  3F          ???
123D  45 15     EOR 15
123F  0F          ???
1240  DB          ???
1241  2C FC 66  BIT 66FC
1244  56 E3     LSR E3,X
1246  F0 79     BEQ 12C1
1248  F5 02     SBC 02,X
124A  2A        ROL A
124B  BA        TSX 
124C  E0 0C     CPX #0C
124E  73          ???
124F  18        CLC 
1250  2A        ROL A
1251  8D 2C 5B  STA 5B2C
1254  EC 11 28  CPX 2811
1257  3D F4 A5  AND A5F4,X
125A  B9 68 DF  LDA DF68,Y
125D  02          ???
125E  18        CLC 
125F  7D 1F F5  ADC F51F,X
1262  4D 81 7A  EOR 7A81
1265  CE 26 0D  DEC 0D26
1268  B8        CLV 
1269  2D 54 70  AND 7054
126C  36 0D     ROL 0D,X
126E  60        RTS 
126F  3E 3A 44  ROL 443A,X
1272  1E 86 8B  ASL 8B86,X
1275  2E 3F 1B  ROL 1B3F
1278  A8        TAY 
1279  65 DF     ADC DF
127B  2B          ???
127C  E2          ???
127D  2C 17 21  BIT 2117
1280  50 8D     BVC 120F
1282  F3          ???
1283  18        CLC 
1284  54          ???
1285  14          ???
1286  5A          ???
1287  DB          ???
1288  D3          ???
1289  B9 CB 4F  LDA 4FCB,Y
128C  74          ???
128D  EC 5E 8D  CPX 8D5E
1290  84 88     STY 88
1292  90 10     BCC 12A4
1294  47          ???
1295  02          ???
1296  E6 0F     INC 0F
1298  D7          ???
1299  77          ???
129A  33          ???
129B  B5 5B     LDA 5B,X
129D  8B          ???
129E  81 10     STA (10,X)
12A0  78        SEI 
12A1  7A          ???
12A2  5B          ???
12A3  06 56     ASL 56
12A5  0C          ???
12A6  89          ???
12A7  B2          ???
12A8  7C          ???
12A9  33          ???
12AA  FA          ???
12AB  29 AA     AND #AA
12AD  0F          ???
12AE  1F          ???
12AF  0C          ???
12B0  11 01     ORA (01),Y
12B2  E2          ???
12B3  4C D6 61  JMP 61D6
12B6  12          ???
12B7  0D 11 2B  ORA 2B11
12BA  03          ???
12BB  8B          ???
12BC  A4 A0     LDY A0
12BE  B5 47     LDA 47,X
12C0  C4 13     CPY 13
12C2  DC          ???
12C3  04          ???
12C4  7C          ???
12C5  0D B5 81  ORA 81B5
12C8  68        PLA 
12C9  7C          ???
12CA  1D DE FC  ORA FCDE,X
12CD  15 91     ORA 91,X
12CF  84 B2     STY B2
12D1  48        PHA 
12D2  CE A8 BC  DEC BCA8
12D5  97          ???
12D6  E1 C1     SBC (C1,X)
12D8  2C 3C E3  BIT E33C
12DB  61 03     ADC (03,X)
12DD  9B          ???
12DE  34          ???
12DF  96 B1     STX B1,Y
12E1  10 90     BPL 1273
12E3  5E 62 4F  LSR 4F62,X
12E6  A3          ???
12E7  CB          ???
12E8  93          ???
12E9  E2          ???
12EA  7A          ???
12EB  52          ???
12EC  36 88     ROL 88,X
12EE  CC 75 B2  CPY B275
12F1  B8        CLV 
12F2  09 4B     ORA #4B
12F4  70 E5     BVS 12DB
12F6  2E 39 F4  ROL F439
12F9  39 53 35  AND 3553,Y
12FC  A6 47     LDX 47
12FE  CB          ???
12FF  13          ???
1300  A2 55     LDX #55
1302  A3          ???
1303  16 BC     ASL BC,X
1305  19 E8 BA  ORA BAE8,Y
1308  1C          ???
1309  4C 13 B9  JMP B913
130C  59 0C E4  EOR E40C,Y
130F  17          ???
1310  08        PHP 
1311  76 2E     ROR 2E,X
1313  0C          ???
1314  8F          ???
1315  C1 F9     CMP (F9,X)
1317  E0 D8     CPX #D8
1319  C4 74     CPY 74
131B  D4          ???
131C  52          ???
131D  C6 8B     DEC 8B
131F  E9 2C     SBC #2C
1321  29 2C     AND #2C
1323  1D 0E F6  ORA F60E,X
1326  5A          ???
1327  6F          ???
1328  48        PHA 
1329  23          ???
132A  19 3F 76  ORA 763F,Y
132D  C5 0F     CMP 0F
132F  13          ???
1330  87          ???
1331  F2          ???
1332  5C          ???
1333  2D D5 C0  AND C0D5
1336  C5 41     CMP 41
1338  B2          ???
1339  7F          ???
133A  78        SEI 
133B  17          ???
133C  0B          ???
133D  6B          ???
133E  82          ???
133F  2D 62 E4  AND E462
1342  DC          ???
1343  47          ???
1344  7B          ???
1345  E2          ???
1346  66 20     ROR 20
1348  AF          ???
1349  A6 25     LDX 25
134B  AF          ???
134C  9B          ???
134D  1A          ???
134E  10 75     BPL 13C5
1350  75 8D     ADC 8D,X
1352  27          ???
1353  C9 9D     CMP #9D
1355  15 C0     ORA C0,X
1357  79 85 0D  ADC 0D85,Y
135A  06 92     ASL 92
135C  4C 18 EA  JMP EA18
135F  D7          ???
1360  01 BB     ORA (BB,X)
1362  98        TYA 
1363  04          ???
1364  DF          ???
1365  12          ???
1366  74          ???
1367  E3          ???
1368  C7          ???
1369  B1 E5     LDA (E5),Y
136B  D5 14     CMP 14,X
136D  27          ???
136E  0F          ???
136F  A9 5A     LDA #5A
1371  97          ???
1372  42          ???
1373  5A          ???
1374  E1 9F     SBC (9F,X)
1376  19 03 1B  ORA 1B03,Y
1379  EF          ???
137A  06 88     ASL 88
137C  7D 04 25  ADC 2504,X
137F  5C          ???
1380  0D 8E 4C  ORA 4C8E
1383  7A          ???
1384  9D 0B C2  STA C20B,X
1387  BF          ???
1388  1D E7 3C  ORA 3CE7,X
138B  D3          ???
138C  E0 8A     CPX #8A
138E  CB          ???
138F  D0 AD     BNE 133E
1391  5E 25 6B  LSR 6B25,X
1394  A7          ???
1395  61 D0     ADC (D0,X)
1397  7E E3 2A  ROR 2AE3,X
139A  C3          ???
139B  72          ???
139C  2F          ???
139D  E8        INX 
139E  BF          ???
139F  71 4D     ADC (4D),Y
13A1  1C          ???
13A2  1A          ???
13A3  4E E3 F3  LSR F3E3
13A6  4F          ???
13A7  36 DB     ROL DB,X
13A9  48        PHA 
13AA  DE 5E A4  DEC A45E,X
13AD  2C 1D A0  BIT A01D
13B0  DF          ???
13B1  A4 3C     LDY 3C
13B3  B7          ???
13B4  E3          ???
13B5  2C B1 E3  BIT E3B1
13B8  B2          ???
13B9  C0 8D     CPY #8D
13BB  CD C6 76  CMP 76C6
13BE  80          ???
13BF  0E 77 16  ASL 1677
13C2  51 C1     EOR (C1),Y
13C4  2C E8 8D  BIT 8DE8
13C7  F5 13     SBC 13,X
13C9  A0 28     LDY #28
13CB  E4 1D     CPX 1D
13CD  07          ???
13CE  8B          ???
13CF  84 56     STY 56
13D1  94 49     STY 49,X
13D3  CC DC 8E  CPY 8EDC
13D6  36 9A     ROL 9A,X
13D8  85 56     STA 56
13DA  C1 5F     CMP (5F,X)
13DC  14          ???
13DD  4B          ???
13DE  74          ???
13DF  40        RTI 
13E0  C5 F0     CMP F0
13E2  49 C7     EOR #C7
13E4  F5 81     SBC 81,X
13E6  CA        DEX 
13E7  B9 8F CA  LDA CA8F,Y
13EA  85 47     STA 47
13EC  20 AD 76  JSR 76AD
13EF  10 F9     BPL 13EA
13F1  64          ???
13F2  CB          ???
13F3  36 7A     ROL 7A,X
13F5  69 74     ADC #74
13F7  60        RTS 
13F8  92          ???
13F9  28        PLP 
13FA  B4 F7     LDY F7,X
13FC  19 87 08  ORA 0887,Y
13FF  1C          ???
1400  03          ???
1401  0E CD 19  ASL 19CD
1404  E5 F8     SBC F8
1406  9F          ???
1407  18        CLC 
1408  6A        ROR A
1409  82          ???
140A  B8        CLV 
140B  DB          ???
140C  1F          ???
140D  46 13     LSR 13
140F  60        RTS 
1410  62          ???
1411  4F          ???
1412  0E 5C B3  ASL B35C
1415  A5 23     LDA 23
1417  A7          ???
1418  AD 4B 71  LDA 714B
141B  84 B0     STY B0
141D  DA          ???
141E  75 18     ADC 18,X
1420  AD 8D 42  LDA 428D
1423  21 A7     AND (A7,X)
1425  E5 DC     SBC DC
1427  02          ???
1428  C1 17     CMP (17,X)
142A  2E 07 73  ROL 7307
142D  F0 61     BEQ 1490
142F  13          ???
1430  D3          ???
1431  28        PLP 
1432  DC          ???
1433  4C 24 05  JMP 0524
1436  A8        TAY 
1437  A1 19     LDA (19,X)
1439  77          ???
143A  F1 0D     SBC (0D),Y
143C  77          ???
143D  67          ???
143E  DD 1D E6  CMP E61D,X
1441  36 FB     ROL FB,X
1443  E1 D0     SBC (D0,X)
1445  9A        TXS 
1446  72          ???
1447  AB          ???
1448  39 87 FF  AND FF87,Y
144B  4C 86 7A  JMP 7A86
144E  6A        ROR A
144F  54          ???
1450  42          ???
1451  1C          ???
1452  50 6C     BVC 14C0
1454  46 E2     LSR E2
1456  8D 02 EA  STA EA02
1459  87          ???
145A  5C          ???
145B  1E 96 02  ASL 0296,X
145E  78        SEI 
145F  B5 B9     LDA B9,X
1461  D2          ???
1462  0B          ???
1463  A3          ???
1464  CF          ???
1465  2C 12 07  BIT 0712
1468  BC BB C5  LDY C5BB,X
146B  4F          ???
146C  9D 68 5F  STA 5F68,X
146F  3B          ???
1470  81 9E     STA (9E,X)
1472  74          ???
1473  18        CLC 
1474  A5 61     LDA 61
1476  F3          ???
1477  26 53     ROL 53
1479  7D C4 89  ADC 89C4,X
147C  5F          ???
147D  1D 0A 5B  ORA 5B0A,X
1480  13          ???
1481  42          ???
1482  DE AE 19  DEC 19AE,X
1485  AE B0 A3  LDX A3B0
1488  85 FC     STA FC
148A  C8        INY 
148B  45 BF     EOR BF
148D  A1 23     LDA (23,X)
148F  D3          ???
1490  07          ???
1491  7A          ???
1492  1B          ???
1493  97          ???
1494  41 FB     EOR (FB,X)
1496  82          ???
1497  F3          ???
1498  EC C8 B7  CPX B7C8
149B  D6 94     DEC 94,X
149D  7C          ???
149E  F8        SED 
149F  FE 34 66  INC 6634,X
14A2  07          ???
14A3  9B          ???
14A4  45 A4     EOR A4
14A6  09 30     ORA #30
14A8  E1 04     SBC (04,X)
14AA  1B          ???
14AB  1F          ???
14AC  3F          ???
14AD  8D 22 DA  STA DA22
14B0  F2          ???
14B1  AB          ???
14B2  D4          ???
14B3  46 36     LSR 36
14B5  07          ???
14B6  CB          ???
14B7  42          ???
14B8  0B          ???
14B9  BF          ???
14BA  42          ???
14BB  1B          ???
14BC  E2          ???
14BD  7C          ???
14BE  46 7B     LSR 7B
14C0  24 5C     BIT 5C
14C2  C0 17     CPY #17
14C4  55 6B     EOR 6B,X
14C6  E7          ???
14C7  43          ???
14C8  FE 20 07  INC 0720,X
14CB  64          ???
14CC  48        PHA 
14CD  D9 1E 7D  CMP 7D1E,Y
14D0  DE C4 7C  DEC 7CC4,X
14D3  72          ???
14D4  6B          ???
14D5  C0 A4     CPY #A4
14D7  07          ???
14D8  E0 FC     CPX #FC
14DA  41 8A     EOR (8A,X)
14DC  EA        NOP 
14DD  DB          ???
14DE  FA          ???
14DF  75 36     ADC 36,X
14E1  C1 1B     CMP (1B,X)
14E3  52          ???
14E4  9A        TXS 
14E5  38        SEC 
14E6  9D 8D 02  STA 028D,X
14E9  9A        TXS 
14EA  6F          ???
14EB  84 26     STY 26
14ED  70 57     BVS 1546
14EF  4F          ???
14F0  83          ???
14F1  DD 1F 15  CMP 151F,X
14F4  CA        DEX 
14F5  F0 AA     BEQ 14A1
14F7  D4          ???
14F8  93          ???
14F9  37          ???
14FA  4C 1A 88  JMP 881A
14FD  97          ???
14FE  CF          ???
14FF  96 F1     STX F1,Y
1501  74          ???
1502  76 30     ROR 30,X
1504  98        TYA 
1505  31 91     AND (91),Y
1507  0E 5B DE  ASL DE5B
150A  51 A0     EOR (A0),Y
150C  D2          ???
150D  AB          ???
150E  2A        ROL A
150F  16 02     ASL 02,X
1511  F2          ???
1512  8E 2A E2  STX E22A
1515  E1 0F     SBC (0F,X)
1517  0E AF 8D  ASL 8DAF
151A  C4 15     CPY 15
151C  74          ???
151D  52          ???
151E  54          ???
151F  36 82     ROL 82,X
1521  30 9D     BMI 14C0
1523  B5 A8     LDA A8,X
1525  EE F7 4E  INC 4EF7
1528  72          ???
1529  A7          ???
152A  1A          ???
152B  4B          ???
152C  CC 02 AD  CPY AD02
152F  F8        SED 
1530  F5 CC     SBC CC,X
1532  C2          ???
1533  40        RTI 
1534  88        DEY 
1535  FF          ???
1536  4F          ???
1537  33          ???
1538  A1 49     LDA (49,X)
153A  29 DE     AND #DE
153C  31 AE     AND (AE),Y
153E  BE 78 86  LDX 8678,Y
1541  03          ???
1542  F3          ???
1543  15 B1     ORA B1,X
1545  33          ???
1546  64          ???
1547  BC DC E4  LDY E4DC,X
154A  A9 5D     LDA #5D
154C  E3          ???
154D  7F          ???
154E  34          ???
154F  4E 71 FA  LSR FA71
1552  8E 2D 27  STX 272D
1555  89          ???
1556  3E 17 F4  ROL F417,X
1559  2B          ???
155A  CA        DEX 
155B  57          ???
155C  70 6D     BVS 15CB
155E  5F          ???
155F  1B          ???
1560  34          ???
1561  7A          ???
1562  2F          ???
1563  45 0C     EOR 0C
1565  06 BB     ASL BB
1567  62          ???
1568  64          ???
1569  5C          ???
156A  BA        TSX 
156B  A3          ???
156C  6D 40 DD  ADC DD40
156F  A3          ???
1570  AA        TAX 
1571  28        PLP 
1572  E5 A3     SBC A3
1574  12          ???
1575  49 A4     EOR #A4
1577  BD 9A F2  LDA F29A,X
157A  C6 FA     DEC FA
157C  41 B4     EOR (B4,X)
157E  C3          ???
157F  A6 D4     LDX D4
1581  B3          ???
1582  A7          ???
1583  BF          ???
1584  59 E6 94  EOR 94E6,Y
1587  5E E9 9F  LSR 9FE9,X
158A  51 BB     EOR (BB),Y
158C  D7          ???
158D  FA          ???
158E  A9 F5     LDA #F5
1590  4F          ???
1591  26 42     ROL 42
1593  27          ???
1594  74          ???
1595  71 A6     ADC (A6),Y
1597  2B          ???
1598  CE 03 08  DEC 0803
159B  08        PHP 
159C  DA          ???
159D  25 8D     AND 8D
159F  9A        TXS 
15A0  35 74     AND 74,X
15A2  38        SEC 
15A3  0B          ???
15A4  38        SEC 
15A5  44          ???
15A6  4D 3E 01  EOR 013E
15A9  56 69     LSR 69,X
15AB  DB          ???
15AC  59 DC 28  EOR 28DC,Y
15AF  FD 50 55  SBC 5550,X
15B2  8D 3D 82  STA 823D
15B5  5E 25 04  LSR 0425,X
15B8  60        RTS 
15B9  4E B9 1E  LSR 1EB9
15BC  7C          ???
15BD  5D AE 29  EOR 29AE,X
15C0  DD 90 D8  CMP D890,X
15C3  0D 14 8F  ORA 8F14
15C6  E8        INX 
15C7  8F          ???
15C8  C8        INY 
15C9  6F          ???
15CA  13          ???
15CB  F0 90     BEQ 155D
15CD  15 4F     ORA 4F,X
15CF  08        PHP 
15D0  6C D4 B1  JMP (B1D4)
15D3  83          ???
15D4  0F          ???
15D5  23          ???
15D6  2E E5 0D  ROL 0DE5
15D9  4D 4C 0B  EOR 0B4C
15DC  62          ???
15DD  DC          ???
15DE  2E 68 94  ROL 9468
15E1  9E          ???
15E2  0B          ???
15E3  98        TYA 
15E4  91 08     STA (08),Y
15E6  C9 D3     CMP #D3
15E8  13          ???
15E9  5A          ???
15EA  06 4E     ASL 4E
15EC  BF          ???
15ED  10 AC     BPL 159B
15EF  78        SEI 
15F0  7E 04 B9  ROR B904,X
15F3  7C          ???
15F4  4D E3 F4  EOR F4E3
15F7  03          ???
15F8  B3          ???
15F9  4C 28 3C  JMP 3C28
15FC  67          ???
15FD  A0 B4     LDY #B4
15FF  76 DA     ROR DA,X
1601  28        PLP 
1602  3B          ???
1603  38        SEC 
1604  0C          ???
1605  31 7C     AND (7C),Y
1607  5B          ???
1608  00        BRK 
1609  5F          ???
160A  0F          ???
160B  C7          ???
160C  0F          ???
160D  10 CE     BPL 15DD
160F  B0 25     BCS 1636
1611  62          ???
1612  A7          ???
1613  BF          ???
1614  99 4C 87  STA 874C,Y
1617  85 A8     STA A8
1619  92          ???
161A  DD AA 10  CMP 10AA,X
161D  AB          ???
161E  02          ???
161F  31 83     AND (83),Y
1621  93          ???
1622  B2          ???
1623  5E 75 2B  LSR 2B75,X
1626  A0 D5     LDY #D5
1628  66 4B     ROR 4B
162A  A2 46     LDX #46
162C  53          ???
162D  3A          ???
162E  3E DE 2B  ROL 2BDE,X
1631  F2          ???
1632  2F          ???
1633  38        SEC 
1634  5F          ???
1635  6C E8 37  JMP (37E8)
1638  66 15     ROR 15
163A  0D 2B 36  ORA 362B
163D  2F          ???
163E  1A          ???
163F  17          ???
1640  94 9F     STY 9F,X
1642  22          ???
1643  4A        LSR A
1644  74          ???
1645  33          ???
1646  6A        ROR A
1647  1B          ???
1648  0E 0C 7A  ASL 7A0C
164B  4C E9 FC  JMP FCE9
164E  B4 AA     LDY AA,X
1650  01 9A     ORA (9A,X)
1652  84 4D     STY 4D
1654  BA        TSX 
1655  81 56     STA (56,X)
1657  34          ???
1658  AB          ???
1659  E1 35     SBC (35,X)
165B  28        PLP 
165C  57          ???
165D  AE CE F9  LDX F9CE
1660  A0 BD     LDY #BD
1662  CE 27 34  DEC 3427
1665  8D F3 45  STA 45F3
1668  9D 74 0E  STA 0E74,X
166B  61 0E     ADC (0E,X)
166D  B4 FB     LDY FB,X
166F  28        PLP 
1670  D6 4D     DEC 4D,X
1672  E0 AF     CPX #AF
1674  3B          ???
1675  71 49     ADC (49),Y
1677  90 D3     BCC 164C
1679  26 0F     ROL 0F
167B  AC A1 9F  LDY 9FA1
167E  03          ???
167F  45 DD     EOR DD
1681  56 BE     LSR BE,X
1683  F5 A7     SBC A7,X
1685  2A        ROL A
1686  0E E3 0A  ASL 0AE3
1689  1D C6 EC  ORA ECC6,X
168C  10 4A     BPL 16D8
168E  CE 22 18  DEC 1822
1691  A3          ???
1692  7C          ???
1693  64          ???
1694  C5 A1     CMP A1
1696  53          ???
1697  13          ???
1698  10 9C     BPL 1636
169A  1A          ???
169B  79 01 4F  ADC 4F01,Y
169E  E5 B1     SBC B1
16A0  3F          ???
16A1  82          ???
16A2  DD 19 39  CMP 3919,X
16A5  E6 53     INC 53
16A7  3E 05 BB  ROL BB05,X
16AA  A7          ???
16AB  EC 99 C5  CPX C599
16AE  CD DE 85  CMP 85DE
16B1  24 FA     BIT FA
16B3  DE 3C 10  DEC 103C,X
16B6  D9 C6 4E  CMP 4EC6,Y
16B9  60        RTS 
16BA  E9 39     SBC #39
16BC  EB          ???
16BD  11 18     ORA (18),Y
16BF  91 16     STA (16),Y
16C1  9A        TXS 
16C2  B8        CLV 
16C3  60        RTS 
16C4  23          ???
16C5  8D E5 CE  STA CEE5
16C8  38        SEC 
16C9  4A        LSR A
16CA  4C 32 D5  JMP D532
16CD  D5 1C     CMP 1C,X
16CF  D3          ???
16D0  C3          ???
16D1  C3          ???
16D2  63          ???
16D3  66 36     ROR 36
16D5  63          ???
16D6  2B          ???
16D7  05 B9     ORA B9
16D9  21 E7     AND (E7,X)
16DB  05 99     ORA 99
16DD  E3          ???
16DE  80          ???
16DF  52          ???
16E0  FA          ???
16E1  8F          ???
16E2  8A        TXA 
16E3  2B          ???
16E4  70 E7     BVS 16CD
16E6  8F          ???
16E7  0C          ???
16E8  2E CC 86  ROL 86CC
16EB  D8        CLD 
16EC  4C 70 88  JMP 8870
16EF  06 9D     ASL 9D
16F1  6B          ???
16F2  25 10     AND 10
16F4  6C 33 5C  JMP (5C33)
16F7  83          ???
16F8  6C 8D 11  JMP (118D)
16FB  0A        ASL A
16FC  74          ???
16FD  F5 32     SBC 32,X
16FF  4C 65 38  JMP 3865
1702  84 43     STY 43
1704  36 2F     ROL 2F,X
1706  7B          ???
1707  8F          ???
1708  53          ???
1709  89          ???
170A  2C 8D 70  BIT 708D
170D  B6 88     LDX 88,Y
170F  02          ???
1710  61 AA     ADC (AA,X)
1712  02          ???
1713  CB          ???
1714  C0 73     CPY #73
1716  C0 98     CPY #98
1718  95 E7     STA E7,X
171A  BC 52 A4  LDY A452,X
171D  4B          ???
171E  23          ???
171F  CC CB 54  CPY 54CB
1722  A5 AD     LDA AD
1724  DF          ???
1725  97          ???
1726  AE F3 C5  LDX C5F3
1729  2F          ???
172A  EB          ???
172B  1D FE E5  ORA E5FE,X
172E  C8        INY 
172F  DA          ???
1730  BE E0 4D  LDX 4DE0,Y
1733  7C          ???
1734  44          ???
1735  E7          ???
1736  42          ???
1737  A0 9A     LDY #9A
1739  FC          ???
173A  2C 9A C9  BIT C99A
173D  0E 8E 2E  ASL 2E8E
1740  0F          ???
1741  C6 1C     DEC 1C
1743  C9 A7     CMP #A7
1745  4F          ???
1746  98        TYA 
1747  BF          ???
1748  8D 8C 38  STA 388C
174B  3F          ???
174C  BD 12 81  LDA 8112,X
174F  D4          ???
1750  D3          ???
1751  6E 24 95  ROR 9524
1754  87          ???
1755  05 F3     ORA F3
1757  6C 1D 0C  JMP (0C1D)
175A  40        RTI 
175B  AF          ???
175C  E4 50     CPX 50
175E  5B          ???
175F  14          ???
1760  56 C1     LSR C1,X
1762  95 56     STA 56,X
1764  37          ???
1765  DE 20 45  DEC 4520,X
1768  4B          ???
1769  9A        TXS 
176A  CE 8E 45  DEC 458E
176D  86 8A     STX 8A
176F  46 CA     LSR CA
1771  05 08     ORA 08
1773  3E 0C 5B  ROL 5B0C,X
1776  34          ???
1777  82          ???
1778  49 D3     EOR #D3
177A  0E C1 1F  ASL 1FC1
177D  6B          ???
177E  3B          ???
177F  E6 8D     INC 8D
1781  C6 4E     DEC 4E
1783  98        TYA 
1784  0A        ASL A
1785  16 4F     ASL 4F,X
1787  DA          ???
1788  04          ???
1789  49 02     EOR #02
178B  43          ???
178C  8F          ???
178D  24 C9     BIT C9
178F  54          ???
1790  74          ???
1791  6C 6A A9  JMP (A96A)
1794  82          ???
1795  BF          ???
1796  39 D3 C2  AND C2D3,Y
1799  91 E2     STA (E2),Y
179B  15 B8     ORA B8,X
179D  2A        ROL A
179E  9D E5 E5  STA E5E5,X
17A1  37          ???
17A2  70 06     BVS 17AA
17A4  97          ???
17A5  B8        CLV 
17A6  53          ???
17A7  C0 61     CPY #61
17A9  08        PHP 
17AA  CC 44 B7  CPY B744
17AD  24 40     BIT 40
17AF  D6 6A     DEC 6A,X
17B1  22          ???
17B2  99 3F DD  STA DD3F,Y
17B5  F1 0D     SBC (0D),Y
17B7  B4 0B     LDY 0B,X
17B9  2D C2 1B  AND 1BC2
17BC  34          ???
17BD  4D BA 48  EOR 48BA
17C0  DB          ???
17C1  41 41     EOR (41,X)
17C3  BB          ???
17C4  56 80     LSR 80,X
17C6  9D BC 48  STA 48BC,X
17C9  25 85     AND 85
17CB  35 53     AND 53,X
17CD  2E 2D 9B  ROL 9B2D
17D0  01 5C     ORA (5C,X)
17D2  4C FA 49  JMP 49FA
17D5  54          ???
17D6  E1 F2     SBC (F2,X)
17D8  50 48     BVC 1822
17DA  70 4B     BVS 1827
17DC  B8        CLV 
17DD  2A        ROL A
17DE  2A        ROL A
17DF  1D AC 84  ORA 84AC,X
17E2  F6 43     INC 43,X
17E4  86 7C     STX 7C
17E6  C2          ???
17E7  D2          ???
17E8  8E B8 38  STX 38B8
17EB  06 CD     ASL CD
17ED  71 61     ADC (61),Y
17EF  2B          ???
17F0  51 61     EOR (61),Y
17F2  9F          ???
17F3  16 0D     ASL 0D,X
17F5  DA          ???
17F6  A2 F8     LDX #F8
17F8  95 64     STA 64,X
17FA  A8        TAY 
17FB  41 0E     EOR (0E,X)
17FD  5B          ???
17FE  AF          ???
17FF  9F          ???
1800  63          ???
1801  90 E0     BCC 17E3
1803  13          ???
1804  21 8B     AND (8B,X)
1806  88        DEY 
1807  E0 2C     CPX #2C
1809  1D 11 0A  ORA 0A11,X
180C  55 F2     EOR F2,X
180E  C3          ???
180F  D1 92     CMP (92),Y
1811  0A        ASL A
1812  91 D1     STA (D1),Y
1814  5E C5 91  LSR 91C5,X
1817  94 20     STY 20,X
1819  A4 C0     LDY C0
181B  77          ???
181C  40        RTI 
181D  5A          ???
181E  12          ???
181F  E1 35     SBC (35,X)
1821  27          ???
1822  09 24     ORA #24
1824  A1 BE     LDA (BE,X)
1826  7C          ???
1827  27          ???
1828  0A        ASL A
1829  92          ???
182A  DD B4 9F  CMP 9FB4,X
182D  1D 19 E7  ORA E719,X
1830  84 B8     STY B8
1832  F9 C8 73  SBC 73C8,Y
1835  4D 92 1F  EOR 1F92
1838  3D 3B 2E  AND 2E3B,X
183B  01 C8     ORA (C8,X)
183D  B2          ???
183E  F7          ???
183F  00        BRK 
1840  49 03     EOR #03
1842  C5 D8     CMP D8
1844  EE AC 43  INC 43AC
1847  C0 2A     CPY #2A
1849  84 15     STY 15
184B  9F          ???
184C  04          ???
184D  02          ???
184E  35 2C     AND 2C,X
1850  9E          ???
1851  89          ???
1852  0B          ???
1853  97          ???
1854  C6 94     DEC 94
1856  B0 E1     BCS 1839
1858  EF          ???
1859  A5 58     LDA 58
185B  FC          ???
185C  0F          ???
185D  B7          ???
185E  42          ???
185F  4B          ???
1860  9C          ???
1861  90 DD     BCC 1840
1863  22          ???
1864  53          ???
1865  CF          ???
1866  0A        ASL A
1867  43          ???
1868  BB          ???
1869  14          ???
186A  B7          ???
186B  22          ???
186C  C5 FD     CMP FD
186E  3C          ???
186F  25 44     AND 44
1871  76 A0     ROR A0,X
1873  46 51     LSR 51
1875  C6 0F     DEC 0F
1877  54          ???
1878  24 1B     BIT 1B
187A  B9 F8 8D  LDA 8DF8,Y
187D  8F          ???
187E  1A          ???
187F  1E 74 F0  ASL F074,X
1882  64          ???
1883  48        PHA 
1884  1B          ???
1885  72          ???
1886  9E          ???
1887  DA          ???
1888  11 5A     ORA (5A),Y
188A  BA        TSX 
188B  48        PHA 
188C  C0 F3     CPY #F3
188E  24 11     BIT 11
1890  0E 5C F2  ASL F25C
1893  60        RTS 
1894  DB          ???
1895  16 D0     ASL D0,X
1897  A1 BC     LDA (BC,X)
1899  E3          ???
189A  2C 70 5D  BIT 5D70
189D  1F          ???
189E  0C          ???
189F  7E 26 0B  ROR 0B26,X
18A2  4A        LSR A
18A3  4F          ???
18A4  73          ???
18A5  0D 28 43  ORA 4328
18A8  72          ???
18A9  09 41     ORA #41
18AB  09 C5     ORA #C5
18AD  03          ???
18AE  74          ???
18AF  4C 4F 0E  JMP 0E4F
18B2  E8        INX 
18B3  0E 55 69  ASL 6955
18B6  38        SEC 
18B7  2E 68 03  ROL 0368
18BA  15 E2     ORA E2,X
18BC  DE 64 05  DEC 0564,X
18BF  28        PLP 
18C0  64          ???
18C1  E4 5F     CPX 5F
18C3  57          ???
18C4  E0 96     CPX #96
18C6  2A        ROL A
18C7  4F          ???
18C8  41 E3     EOR (E3,X)
18CA  16 05     ASL 05,X
18CC  0A        ASL A
18CD  19 4F 2D  ORA 2D4F,Y
18D0  38        SEC 
18D1  07          ???
18D2  8C 69 00  STY 0069
18D5  90 90     BCC 1867
18D7  EF          ???
18D8  C8        INY 
18D9  E7          ???
18DA  A6 22     LDX 22
18DC  46 B5     LSR B5
18DE  59 59 86  EOR 8659,Y
18E1  41 39     EOR (39,X)
18E3  CE 57 22  DEC 2257
18E6  4F          ???
18E7  64          ???
18E8  61 10     ADC (10,X)
18EA  E4 E3     CPX E3
18EC  0E E3 6D  ASL 6DE3
18EF  50 F5     BVC 18E6
18F1  34          ???
18F2  C9 B2     CMP #B2
18F4  1A          ???
18F5  CF          ???
18F6  CC F9 01  CPY 01F9
18F9  DC          ???
18FA  43          ???
18FB  97          ???
18FC  61 E1     ADC (E1,X)
18FE  CE 39 1C  DEC 1C39
1901  76 10     ROR 10,X
1903  59 FE 54  EOR 54FE,Y
1906  BB          ???
1907  A8        TAY 
1908  32          ???
1909  39 F7 CD  AND CDF7,Y
190C  78        SEI 
190D  37          ???
190E  2E 9F 60  ROL 609F
1911  4F          ???
1912  36 03     ROL 03,X
1914  E6 49     INC 49
1916  BA        TSX 
1917  C2          ???
1918  54          ???
1919  7F          ???
191A  E6 25     INC 25
191C  2C A3 F7  BIT F7A3
191F  C7          ???
1920  29 14     AND #14
1922  5B          ???
1923  73          ???
1924  2D 0D 24  AND 240D
1927  91 D3     STA (D3),Y
1929  C0 3F     CPY #3F
192B  10 EF     BPL 191C
192D  DF          ???
192E  4D 2E 71  EOR 712E
1931  00        BRK 
1932  FA          ???
1933  45 EA     EOR EA
1935  AD E8 12  LDA 12E8
1938  56 D9     LSR D9,X
193A  98        TYA 
193B  8D 6D 13  STA 136D
193E  12          ???
193F  ED 10 51  SBC 5110
1942  14          ???
1943  41 D3     EOR (D3,X)
1945  C8        INY 
1946  39 6C 8B  AND 8B6C,Y
1949  CD 0B C4  CMP C40B
194C  3D B1 AD  AND ADB1,X
194F  8D 5E EA  STA EA5E
1952  E7          ???
1953  3E 48 3A  ROL 3A48,X
1956  76 E4     ROR E4,X
1958  14          ???
1959  D6 6E     DEC 6E,X
195B  5A          ???
195C  3B          ???
195D  24 D6     BIT D6
195F  8D A9 93  STA 93A9
1962  E2          ???
1963  32          ???
1964  BC 9D 46  LDY 469D,X
1967  A2 1E     LDX #1E
1969  03          ???
196A  7A          ???
196B  07          ???
196C  7B          ???
196D  8C 1D 53  STY 531D
1970  FA          ???
1971  04          ???
1972  81 E1     STA (E1,X)
1974  60        RTS 
1975  65 D4     ADC D4
1977  C6 81     DEC 81
1979  77          ???
197A  E1 B8     SBC (B8,X)
197C  17          ???
197D  7E 8B 91  ROR 918B,X
1980  06 E9     ASL E9
1982  CF          ???
1983  3E 63 64  ROL 6463,X
1986  E7          ???
1987  15 55     ORA 55,X
1989  ED 9E 12  SBC 129E
198C  0E D9 8E  ASL 8ED9
198F  DD D4 6D  CMP 6DD4,X
1992  06 65     ASL 65
1994  0D 45 16  ORA 1645
1997  5E D6 39  LSR 39D6,X
199A  38        SEC 
199B  19 C9 E0  ORA E0C9,Y
199E  E8        INX 
199F  9B          ???
19A0  5F          ???
19A1  97          ???
19A2  85 E4     STA E4
19A4  C5 B3     CMP B3
19A6  BA        TSX 
19A7  BB          ???
19A8  3D 5F 00  AND 005F,X
19AB  15 DF     ORA DF,X
19AD  6A        ROR A
19AE  45 FC     EOR FC
19B0  E0 12     CPX #12
19B2  66 4A     ROR 4A
19B4  1E 1C A3  ASL A31C,X
19B7  4E 92 40  LSR 4092
19BA  E4 02     CPX 02
19BC  C6 D8     DEC D8
19BE  0D 93 F5  ORA F593
19C1  57          ???
19C2  B1 EA     LDA (EA),Y
19C4  84 F3     STY F3
19C6  55 2E     EOR 2E,X
19C8  91 FD     STA (FD),Y
19CA  1C          ???
19CB  61 A1     ADC (A1,X)
19CD  9C          ???
19CE  42          ???
19CF  3C          ???
19D0  B3          ???
19D1  8D 54 39  STA 3954
19D4  D7          ???
19D5  51 52     EOR (52),Y
19D7  DF          ???
19D8  09 06     ORA #06
19DA  71 F3     ADC (F3),Y
19DC  CF          ???
19DD  5A          ???
19DE  AD 37 80  LDA 8037
19E1  72          ???
19E2  5A          ???
19E3  C8        INY 
19E4  0F          ???
19E5  41 94     EOR (94,X)
19E7  15 29     ORA 29,X
19E9  02          ???
19EA  B6 85     LDX 85,Y
19EC  FD 27 AE  SBC AE27,X
19EF  1A          ???
19F0  1D 84 70  ORA 7084,X
19F3  D2          ???
19F4  08        PHP 
19F5  DC          ???
19F6  E0 A7     CPX #A7
19F8  67          ???
19F9  21 B8     AND (B8,X)
19FB  C9 F1     CMP #F1
19FD  36 61     ROL 61,X
19FF  9C          ???
1A00  64          ???
1A01  0E AF 50  ASL 50AF
1A04  E0 BC     CPX #BC
1A06  4F          ???
1A07  C6 43     DEC 43
1A09  A5 EC     LDA EC
1A0B  BC 90 F5  LDY F590,X
1A0E  52          ???
1A0F  9D 62 12  STA 1262,X
1A12  6B          ???
1A13  E0 47     CPX #47
1A15  10 97     BPL 19AE
1A17  FB          ???
1A18  B8        CLV 
1A19  70 F7     BVS 1A12
1A1B  B2          ???
1A1C  A9 6B     LDA #6B
1A1E  CF          ???
1A1F  83          ???
1A20  43          ???
1A21  B0 20     BCS 1A43
1A23  FE 0C A8  INC A80C,X
1A26  BB          ???
1A27  C0 17     CPY #17
1A29  74          ???
1A2A  03          ???
1A2B  EE 52 6D  INC 6D52
1A2E  B5 0E     LDA 0E,X
1A30  19 0D 79  ORA 790D,Y
1A33  91 A2     STA (A2),Y
1A35  5D C0 EA  EOR EAC0,X
1A38  9E          ???
1A39  80          ???
1A3A  D6 50     DEC 50,X
1A3C  32          ???
1A3D  65 32     ADC 32
1A3F  3A          ???
1A40  67          ???
1A41  39 11 C5  AND C511,Y
1A44  4D F5 13  EOR 13F5
1A47  5B          ???
1A48  22          ???
1A49  A0 55     LDY #55
1A4B  80          ???
1A4C  69 BE     ADC #BE
1A4E  E3          ???
1A4F  52          ???
1A50  C6 D8     DEC D8
1A52  B1 A8     LDA (A8),Y
1A54  2C 56 DB  BIT DB56
1A57  2D D1 C7  AND C7D1
1A5A  57          ???
1A5B  F6 44     INC 44,X
1A5D  E2          ???
1A5E  22          ???
1A5F  CE F1 52  DEC 52F1
1A62  8C A1 E2  STY E2A1
1A65  0F          ???
1A66  09 41     ORA #41
1A68  0B          ???
1A69  43          ???
1A6A  D3          ???
1A6B  2E 5F 15  ROL 155F
1A6E  9B          ???
1A6F  A0 EB     LDY #EB
1A71  CE 80 F3  DEC F380
1A74  0D 62 0E  ORA 0E62
1A77  12          ???
1A78  68        PLA 
1A79  14          ???
1A7A  CA        DEX 
1A7B  85 0D     STA 0D
1A7D  81 46     STA (46,X)
1A7F  74          ???
1A80  3B          ???
1A81  57          ???
1A82  56 42     LSR 42,X
1A84  C3          ???
1A85  54          ???
1A86  32          ???
1A87  8E 13 7A  STX 7A13
1A8A  6B          ???
1A8B  0A        ASL A
1A8C  11 D0     ORA (D0),Y
1A8E  78        SEI 
1A8F  3E 45 9C  ROL 9C45,X
1A92  58        CLI 
1A93  D6 4D     DEC 4D,X
1A95  A9 CE     LDA #CE
1A97  CB          ???
1A98  B3          ???
1A99  01 39     ORA (39,X)
1A9B  D5 1F     CMP 1F,X
1A9D  2C 72 46  BIT 4672
1AA0  85 05     STA 05
1AA2  D9 24 16  CMP 1624,Y
1AA5  D2          ???
1AA6  68        PLA 
1AA7  8D E6 F0  STA F0E6
1AAA  5C          ???
1AAB  71 75     ADC (75),Y
1AAD  04          ???
1AAE  B5 D4     LDA D4,X
1AB0  6D 27 35  ADC 3527
1AB3  4A        LSR A
1AB4  8B          ???
1AB5  47          ???
1AB6  93          ???
1AB7  9F          ???
1AB8  D2          ???
1AB9  9A        TXS 
1ABA  FD 83 94  SBC 9483,X
1ABD  D9 CB E2  CMP E2CB,Y
1AC0  E8        INX 
1AC1  49 4F     EOR #4F
1AC3  3F          ???
1AC4  82          ???
1AC5  6B          ???
1AC6  A0 0D     LDY #0D
1AC8  17          ???
1AC9  2B          ???
1ACA  44          ???
1ACB  B0 4E     BCS 1B1B
1ACD  39 E8 16  AND 16E8,Y
1AD0  1F          ???
1AD1  52          ???
1AD2  EC 28 95  CPX 9528
1AD5  8E 01 BC  STX BC01
1AD8  B8        CLV 
1AD9  92          ???
1ADA  77          ???
1ADB  A9 27     LDA #27
1ADD  90 E1     BCC 1AC0
1ADF  69 C9     ADC #C9
1AE1  A2 C9     LDX #C9
1AE3  49 18     EOR #18
1AE5  D9 8F 63  CMP 638F,Y
1AE8  87          ???
1AE9  12          ???
1AEA  2E D5 9C  ROL 9CD5
1AED  A4 F9     LDY F9
1AEF  7F          ???
1AF0  6E 1E FA  ROR FA1E
1AF3  23          ???
1AF4  8C 2B 59  STY 592B
1AF7  34          ???
1AF8  13          ???
1AF9  8B          ???
1AFA  7F          ???
1AFB  7E D6 D7  ROR D7D6,X
1AFE  34          ???
1AFF  4A        LSR A
1B00  FC          ???
1B01  12          ???
1B02  7D 9A F5  ADC F59A,X
1B05  C4 F1     CPY F1
1B07  8D 07 48  STA 4807
1B0A  C3          ???
1B0B  FC          ???
1B0C  E2          ???
1B0D  19 BB 5C  ORA 5CBB,Y
1B10  DF          ???
1B11  5D 85 2D  EOR 2D85,X
1B14  45 A0     EOR A0
1B16  3A          ???
1B17  71 BC     ADC (BC),Y
1B19  6D D6 16  ADC 16D6
1B1C  2D 8D 0B  AND 0B8D
1B1F  DC          ???
1B20  3C          ???
1B21  7F          ???
1B22  6D 9B 4E  ADC 4E9B
1B25  B9 81 29  LDA 2981,Y
1B28  56 0D     LSR 0D,X
1B2A  0D 2A E6  ORA E62A
1B2D  51 7B     EOR (7B),Y
1B2F  30 84     BMI 1AB5
1B31  78        SEI 
1B32  B5 0B     LDA 0B,X
1B34  06 8E     ASL 8E
1B36  C0 C7     CPY #C7
1B38  00        BRK 
1B39  0F          ???
1B3A  07          ???
1B3B  C7          ???
1B3C  1C          ???
1B3D  73          ???
1B3E  06 A7     ASL A7
1B40  35 8D     AND 8D,X
1B42  C2          ???
1B43  EB          ???
1B44  E4 62     CPX 62
1B46  0E 18 4A  ASL 4A18
1B49  CC BB 65  CPY 65BB
1B4C  E3          ???
1B4D  FE BA 52  INC 52BA,X
1B50  6A        ROR A
1B51  AA        TAX 
1B52  58        CLI 
1B53  87          ???
1B54  A9 DD     LDA #DD
1B56  D0 3E     BNE 1B96
1B58  E4 06     CPX 06
1B5A  67          ???
1B5B  4D 74 BD  EOR BD74
1B5E  96 84     STX 84,Y
1B60  8D 74 07  STA 0774
1B63  1A          ???
1B64  08        PHP 
1B65  D3          ???
1B66  48        PHA 
1B67  72          ???
1B68  8F          ???
1B69  B2          ???
1B6A  78        SEI 
1B6B  19 9D 27  ORA 279D,Y
1B6E  80          ???
1B6F  E8        INX 
1B70  4B          ???
1B71  61 22     ADC (22,X)
1B73  8D 38 08  STA 0838
1B76  E5 23     SBC 23
1B78  33          ???
1B79  30 B7     BMI 1B32
1B7B  BC 68 35  LDY 3568,X
1B7E  86 91     STX 91
1B80  13          ???
1B81  F3          ???
1B82  9D 2C 6F  STA 6F2C,X
1B85  7B          ???
1B86  02          ???
1B87  24 48     BIT 48
1B89  C8        INY 
1B8A  83          ???
1B8B  C7          ???
1B8C  4E 87 7A  LSR 7A87
1B8F  8F          ???
1B90  DC          ???
1B91  74          ???
1B92  59 82 AE  EOR AE82,Y
1B95  AA        TAX 
1B96  1A          ???
1B97  96 E6     STX E6,Y
1B99  47          ???
1B9A  85 98     STA 98
1B9C  B4 83     LDY 83,X
1B9E  1A          ???
1B9F  04          ???
1BA0  57          ???
1BA1  33          ???
1BA2  14          ???
1BA3  93          ???
1BA4  6D 2E 22  ADC 222E
1BA7  FB          ???
1BA8  8E 90 AB  STX AB90
1BAB  54          ???
1BAC  4F          ???
1BAD  55 52     EOR 52,X
1BAF  EF          ???
1BB0  41 C5     EOR (C5,X)
1BB2  4D A1 11  EOR 11A1
1BB5  ED 70 E7  SBC E770
1BB8  40        RTI 
1BB9  B7          ???
1BBA  83          ???
1BBB  D8        CLD 
1BBC  3B          ???
1BBD  52          ???
1BBE  4C 62 8E  JMP 8E62
1BC1  2D C8 AB  AND ABC8
1BC4  E0 3B     CPX #3B
1BC6  D5 8D     CMP 8D,X
1BC8  A1 EE     LDA (EE,X)
1BCA  C4 F2     CPY F2
1BCC  70 7B     BVS 1C49
1BCE  EC 10 01  CPX 0110
1BD1  F2          ???
1BD2  D5 38     CMP 38,X
1BD4  AB          ???
1BD5  E5 26     SBC 26
1BD7  08        PHP 
1BD8  C3          ???
1BD9  B5 53     LDA 53,X
1BDB  51 FD     EOR (FD),Y
1BDD  B8        CLV 
1BDE  49 4A     EOR #4A
1BE0  00        BRK 
1BE1  59 17 EC  EOR EC17,Y
1BE4  85 E6     STA E6
1BE6  ED 58 EF  SBC EF58
1BE9  98        TYA 
1BEA  2D 40 B5  AND B540
1BED  68        PLA 
1BEE  30 AC     BMI 1B9C
1BF0  D5 90     CMP 90,X
1BF2  55 AF     EOR AF,X
1BF4  00        BRK 
1BF5  DA          ???
1BF6  64          ???
1BF7  C8        INY 
1BF8  E5 6E     SBC 6E
1BFA  DD 95 0D  CMP 0D95,X
1BFD  D1 A3     CMP (A3),Y
1BFF  8D 8C 51  STA 518C
1C02  79 9D DF  ADC DF9D,Y
1C05  BA        TSX 
1C06  32          ???
1C07  8B          ???
1C08  46 C9     LSR C9
1C0A  5A          ???
1C0B  C2          ???
1C0C  0B          ???
1C0D  53          ???
1C0E  71 7A     ADC (7A),Y
1C10  6E CB 80  ROR 80CB
1C13  A3          ???
1C14  44          ???
1C15  26 03     ROL 03
1C17  49 70     EOR #70
1C19  9E          ???
1C1A  16 4E     ASL 4E,X
1C1C  EA        NOP 
1C1D  EF          ???
1C1E  6B          ???
1C1F  75 FC     ADC FC,X
1C21  A9 AA     LDA #AA
1C23  AE 08 92  LDX 9208
1C26  10 D2     BPL 1BFA
1C28  C7          ???
1C29  EF          ???
1C2A  13          ???
1C2B  55 07     EOR 07,X
1C2D  AC 43 3A  LDY 3A43
1C30  28        PLP 
1C31  5E 57 87  LSR 8757,X
1C34  3A          ???
1C35  09 8E     ORA #8E
1C37  33          ???
1C38  B2          ???
1C39  19 0B 1E  ORA 1E0B,Y
1C3C  9E          ???
1C3D  C0 A3     CPY #A3
1C3F  40        RTI 
1C40  01 EA     ORA (EA,X)
1C42  65 74     ADC 74
1C44  90 F3     BCC 1C39
1C46  E2          ???
1C47  8F          ???
1C48  34          ???
1C49  41 5C     EOR (5C,X)
1C4B  A5 3E     LDA 3E
1C4D  9E          ???
1C4E  ED 43 7A  SBC 7A43
1C51  A0 AB     LDY #AB
1C53  BE 48 F4  LDX F448,Y
1C56  29 A9     AND #A9
1C58  0D 49 03  ORA 0349
1C5B  A6 42     LDX 42
1C5D  D0 9B     BNE 1BFA
1C5F  4F          ???
1C60  1E 08 11  ASL 1108,X
1C63  05 9D     ORA 9D
1C65  19 AC 91  ORA 91AC,Y
1C68  10 6D     BPL 1CD7
1C6A  DF          ???
1C6B  89          ???
1C6C  BA        TSX 
1C6D  48        PHA 
1C6E  19 A8 08  ORA 08A8,Y
1C71  E1 16     SBC (16,X)
1C73  0A        ASL A
1C74  3F          ???
1C75  6C 5B 4A  JMP (4A5B)
1C78  32          ???
1C79  82          ???
1C7A  97          ???
1C7B  43          ???
1C7C  DD 10 D1  CMP D110,X
1C7F  AB          ???
1C80  26 54     ROL 54
1C82  1A          ???
1C83  9D D4 1E  STA 1ED4,X
1C86  95 B3     STA B3,X
1C88  32          ???
1C89  94 87     STY 87,X
1C8B  2A        ROL A
1C8C  CC FF 8C  CPY 8CFF
1C8F  51 05     EOR (05),Y
1C91  86 3A     STX 3A
1C93  56 4F     LSR 4F,X
1C95  14          ???
1C96  1D 44 4F  ORA 4F44,X
1C99  0B          ???
1C9A  9E          ???
1C9B  46 50     LSR 50
1C9D  E1 15     SBC (15,X)
1C9F  90 3B     BCC 1CDC
1CA1  4F          ???
1CA2  ED 24 09  SBC 0924
1CA5  F7          ???
1CA6  72          ???
1CA7  DC          ???
1CA8  47          ???
1CA9  D3          ???
1CAA  52          ???
1CAB  01 C7     ORA (C7,X)
1CAD  C8        INY 
1CAE  92          ???
1CAF  13          ???
1CB0  37          ???
1CB1  71 0A     ADC (0A),Y
1CB3  CD D0 24  CMP 24D0
1CB6  49 E4     EOR #E4
1CB8  08        PHP 
1CB9  87          ???
1CBA  A3          ???
1CBB  4B          ???
1CBC  5F          ???
1CBD  5A          ???
1CBE  AA        TAX 
1CBF  8C E6 7B  STY 7BE6
1CC2  0C          ???
1CC3  7C          ???
1CC4  41 2D     EOR (2D,X)
1CC6  39 72 2A  AND 2A72,Y
1CC9  2D 0A 30  AND 300A
1CCC  D9 CA D8  CMP D8CA,Y
1CCF  41 C8     EOR (C8,X)
1CD1  67          ???
1CD2  BC 12 48  LDY 4812,X
1CD5  48        PHA 
1CD6  A4 39     LDY 39
1CD8  DB          ???
1CD9  72          ???
1CDA  F0 3F     BEQ 1D1B
1CDC  64          ???
1CDD  5E 3A 74  LSR 743A,X
1CE0  CE 8B 30  DEC 308B
1CE3  F1 D2     SBC (D2),Y
1CE5  2E 81 0E  ROL 0E81
1CE8  D1 34     CMP (34),Y
1CEA  13          ???
1CEB  52          ???
1CEC  6E 60 F8  ROR F860
1CEF  34          ???
1CF0  01 C0     ORA (C0,X)
1CF2  16 11     ASL 11,X
1CF4  9B          ???
1CF5  1C          ???
1CF6  69 47     ADC #47
1CF8  7C          ???
1CF9  C7          ???
1CFA  D0 3F     BNE 1D3B
1CFC  41 09     EOR (09,X)
1CFE  E8        INX 
1CFF  CA        DEX 
1D00  0F          ???
1D01  CB          ???
1D02  64          ???
1D03  D4          ???
1D04  AE D0 B1  LDX B1D0
1D07  B4 3D     LDY 3D,X
1D09  1B          ???
1D0A  19 0B B1  ORA B10B,Y
1D0D  61 D3     ADC (D3,X)
1D0F  3E 1F 04  ROL 041F,X
1D12  A2 B3     LDX #B3
1D14  34          ???
1D15  A3          ???
1D16  80          ???
1D17  5A          ???
1D18  40        RTI 
1D19  8E A8 9B  STX 9BA8
1D1C  B1 18     LDA (18),Y
1D1E  43          ???
1D1F  D9 D0 EE  CMP EED0,Y
1D22  3C          ???
1D23  FA          ???
1D24  1C          ???
1D25  19 85 D5  ORA D585,Y
1D28  A4 DB     LDY DB
1D2A  26 8D     ROL 8D
1D2C  67          ???
1D2D  AA        TAX 
1D2E  16 F6     ASL F6,X
1D30  71 7F     ADC (7F),Y
1D32  03          ???
1D33  D9 51 77  CMP 7751,Y
1D36  2D 77 CB  AND CB77
1D39  00        BRK 
1D3A  A8        TAY 
1D3B  56 DC     LSR DC,X
1D3D  57          ???
1D3E  CB          ???
1D3F  49 50     EOR #50
1D41  D7          ???
1D42  38        SEC 
1D43  D8        CLD 
1D44  4D 3D D3  EOR D33D
1D47  C0 EC     CPY #EC
1D49  8D A2 4E  STA 4EA2
1D4C  2A        ROL A
1D4D  62          ???
1D4E  1B          ???
1D4F  64          ???
1D50  1A          ???
1D51  B4 4D     LDY 4D,X
1D53  27          ???
1D54  A5 E5     LDA E5
1D56  97          ???
1D57  47          ???
1D58  87          ???
1D59  42          ???
1D5A  FB          ???
1D5B  4D 34 E1  EOR E134
1D5E  E2          ???
1D5F  48        PHA 
1D60  D6 94     DEC 94,X
1D62  78        SEI 
1D63  0D 93 9D  ORA 9D93
1D66  4A        LSR A
1D67  C1 68     CMP (68,X)
1D69  59 05 98  EOR 9805,Y
1D6C  C0 8D     CPY #8D
1D6E  53          ???
1D6F  B9 E0 19  LDA 19E0,Y
1D72  C8        INY 
1D73  33          ???
1D74  8D 03 10  STA 1003
1D77  AC 16 59  LDY 5916
1D7A  8E 2D 96  STX 962D
1D7D  1D D7 31  ORA 31D7,X
1D80  A5 D3     LDA D3
1D82  F8        SED 
1D83  73          ???
1D84  16 15     ASL 15,X
1D86  99 DD DE  STA DEDD,Y
1D89  54          ???
1D8A  97          ???
1D8B  72          ???
1D8C  BB          ???
1D8D  88        DEY 
1D8E  0D 37 39  ORA 3937
1D91  89          ???
1D92  E0 E5     CPX #E5
1D94  7C          ???
1D95  BE 2D 1A  LDX 1A2D,Y
1D98  41 99     EOR (99,X)
1D9A  CD E4 58  CMP 58E4
1D9D  88        DEY 
1D9E  B7          ???
1D9F  3A          ???
1DA0  76 38     ROR 38,X
1DA2  67          ???
1DA3  CD 1C 94  CMP 941C
1DA6  42          ???
1DA7  A8        TAY 
1DA8  7B          ???
1DA9  BE A9 29  LDX 29A9,Y
1DAC  0E 48 33  ASL 3348
1DAF  55 C4     EOR C4,X
1DB1  75 15     ADC 15,X
1DB3  8D A1 4A  STA 4AA1
1DB6  29 9B     AND #9B
1DB8  C8        INY 
1DB9  35 50     AND 50,X
1DBB  78        SEI 
1DBC  54          ???
1DBD  2D C9 1D  AND 1DC9
1DC0  9C          ???
1DC1  2E 39 DB  ROL DB39
1DC4  8B          ???
1DC5  83          ???
1DC6  0E D6 5E  ASL 5ED6
1DC9  3D 19 8D  AND 8D19,X
1DCC  CF          ???
1DCD  13          ???
1DCE  0F          ???
1DCF  81 63     STA (63,X)
1DD1  5A          ???
1DD2  5F          ???
1DD3  01 D2     ORA (D2,X)
1DD5  7C          ???
1DD6  D7          ???
1DD7  B6 0A     LDX 0A,Y
1DD9  4A        LSR A
1DDA  F9 C8 EA  SBC EAC8,Y
1DDD  47          ???
1DDE  56 FA     LSR FA,X
1DE0  9D D1 53  STA 53D1,X
1DE3  B8        CLV 
1DE4  A8        TAY 
1DE5  5C          ???
1DE6  05 10     ORA 10
1DE8  5D 40 47  EOR 4740,X
1DEB  C3          ???
1DEC  58        CLI 
1DED  7D 82 14  ADC 1482,X
1DF0  65 57     ADC 57
1DF2  5A          ???
1DF3  74          ???
1DF4  37          ???
1DF5  C5 D2     CMP D2
1DF7  ED 97 2C  SBC 2C97
1DFA  5D 95 23  EOR 2395,X
1DFD  F0 19     BEQ 1E18
1DFF  B4 CC     LDY CC,X
1E01  C5 56     CMP 56
1E03  4C 14 15  JMP 1514
1E06  25 4A     AND 4A
1E08  DE 60 07  DEC 0760,X
1E0B  1C          ???
1E0C  E8        INX 
1E0D  7C          ???
1E0E  5C          ???
1E0F  1B          ???
1E10  06 0A     ASL 0A
1E12  51 D3     EOR (D3),Y
1E14  02          ???
1E15  E1 19     SBC (19,X)
1E17  71 BB     ADC (BB),Y
1E19  A8        TAY 
1E1A  7C          ???
1E1B  07          ???
1E1C  A0 64     LDY #64
1E1E  67          ???
1E1F  02          ???
1E20  4A        LSR A
1E21  ED B9 BC  SBC BCB9
1E24  A3          ???
1E25  B8        CLV 
1E26  E8        INX 
1E27  91 2A     STA (2A),Y
1E29  52          ???
1E2A  92          ???
1E2B  2C 19 F9  BIT F919
1E2E  0E 24 98  ASL 9824
1E31  76 03     ROR 03,X
1E33  BE 6F 83  LDX 836F,Y
1E36  70 27     BVS 1E5F
1E38  39 CF 94  AND 94CF,Y
1E3B  D0 47     BNE 1E84
1E3D  6A        ROR A
1E3E  CB          ???
1E3F  81 D0     STA (D0,X)
1E41  E1 70     SBC (70,X)
1E43  66 EA     ROR EA
1E45  FA          ???
1E46  AC CE 05  LDY 05CE
1E49  A1 44     LDA (44,X)
1E4B  4D E9 EB  EOR EBE9
1E4E  3C          ???
1E4F  3F          ???
1E50  2F          ???
1E51  25 C0     AND C0
1E53  66 60     ROR 60
1E55  73          ???
1E56  4F          ???
1E57  32          ???
1E58  FA          ???
1E59  02          ???
1E5A  9F          ???
1E5B  CC C1 66  CPY 66C1
1E5E  A7          ???
1E5F  03          ???
1E60  41 EC     EOR (EC,X)
1E62  35 B6     AND B6,X
1E64  6E A0 68  ROR 68A0
1E67  E4 41     CPX 41
1E69  86 BD     STX BD
1E6B  B0 BE     BCS 1E2B
1E6D  4A        LSR A
1E6E  EA        NOP 
1E6F  39 50 FA  AND FA50,Y
1E72  49 6E     EOR #6E
1E74  9E          ???
1E75  03          ???
1E76  D1 B8     CMP (B8),Y
1E78  B4 25     LDY 25,X
1E7A  E6 85     INC 85
1E7C  F7          ???
1E7D  87          ???
1E7E  21 8D     AND (8D,X)
1E80  83          ???
1E81  13          ???
1E82  4A        LSR A
1E83  B2          ???
1E84  BE 7E A4  LDX A47E,Y
1E87  4C B7 D5  JMP D5B7
1E8A  05 8A     ORA 8A
1E8C  8D 87 0E  STA 0E87
1E8F  66 C8     ROR C8
1E91  59 7E 38  EOR 387E,Y
1E94  4E 33 FA  LSR FA33
1E97  B6 72     LDX 72,Y
1E99  EB          ???
1E9A  B3          ???
1E9B  F5 D5     SBC D5,X
1E9D  49 9A     EOR #9A
1E9F  ED 8F 2C  SBC 2C8F
1EA2  4E 52 50  LSR 5052
1EA5  E9 D1     SBC #D1
1EA7  41 EB     EOR (EB,X)
1EA9  6C 0A 84  JMP (840A)
1EAC  3E 44 43  ROL 4344,X
1EAF  5B          ???
1EB0  F8        SED 
1EB1  91 A2     STA (A2),Y
1EB3  5D 05 CC  EOR CC05,X
1EB6  92          ???
1EB7  26 7E     ROL 7E
1EB9  05 4B     ORA 4B
1EBB  BB          ???
1EBC  02          ???
1EBD  AC 4D 68  LDY 684D
1EC0  4A        LSR A
1EC1  C6 D6     DEC D6
1EC3  69 C3     ADC #C3
1EC5  8B          ???
1EC6  DD C5 38  CMP 38C5,X
1EC9  7C          ???
1ECA  6C 23 C4  JMP (C423)
1ECD  A6 50     LDX 50
1ECF  37          ???
1ED0  82          ???
1ED1  30 2D     BMI 1F00
1ED3  F1 84     SBC (84),Y
1ED5  16 A1     ASL A1,X
1ED7  F7          ???
1ED8  70 1D     BVS 1EF7
1EDA  2C 07 A6  BIT A607
1EDD  0D EC 26  ORA 26EC
1EE0  58        CLI 
1EE1  08        PHP 
1EE2  9E          ???
1EE3  13          ???
1EE4  32          ???
1EE5  D6 E0     DEC E0,X
1EE7  34          ???
1EE8  3A          ???
1EE9  4B          ???
1EEA  0D 47 9C  ORA 9C47
1EED  22          ???
1EEE  17          ???
1EEF  0B          ???
1EF0  5C          ???
1EF1  98        TYA 
1EF2  FF          ???
1EF3  41 A6     EOR (A6,X)
1EF5  43          ???
1EF6  6F          ???
1EF7  41 C3     EOR (C3,X)
1EF9  13          ???
1EFA  59 03 D4  EOR D403,Y
1EFD  CA        DEX 
1EFE  F9 E3 57  SBC 57E3,Y
1F01  AE D2 0E  LDX 0ED2
1F04  56 34     LSR 34,X
1F06  E0 53     CPX #53
1F08  A5 87     LDA 87
1F0A  46 04     LSR 04
1F0C  9E          ???
1F0D  C6 E9     DEC E9
1F0F  94 91     STY 91,X
1F11  4C 19 E9  JMP E919
1F14  A5 28     LDA 28
1F16  1C          ???
1F17  92          ???
1F18  60        RTS 
1F19  1C          ???
1F1A  9A        TXS 
1F1B  43          ???
1F1C  D7          ???
1F1D  B1 07     LDA (07),Y
1F1F  6D 9D 8C  ADC 8C9D
1F22  4A        LSR A
1F23  4C BA 4A  JMP 4ABA
1F26  5F          ???
1F27  BB          ???
1F28  18        CLC 
1F29  AA        TAX 
1F2A  2A        ROL A
1F2B  30 59     BMI 1F86
1F2D  41 D1     EOR (D1,X)
1F2F  92          ???
1F30  93          ???
1F31  B1 9A     LDA (9A),Y
1F33  BD DE 10  LDA 10DE,X
1F36  B9 3B 15  LDA 153B,Y
1F39  5E EE 08  LSR 08EE,X
1F3C  5F          ???
1F3D  C8        INY 
1F3E  D8        CLD 
1F3F  60        RTS 
1F40  43          ???
1F41  2A        ROL A
1F42  78        SEI 
1F43  26 A8     ROL A8
1F45  54          ???
1F46  9C          ???
1F47  61 75     ADC (75,X)
1F49  0E 73 62  ASL 6273
1F4C  2E 1B E0  ROL E01B
1F4F  17          ???
1F50  32          ???
1F51  C0 72     CPY #72
1F53  EE 31 81  INC 8131
1F56  BE 35 55  LDX 5535,Y
1F59  0C          ???
1F5A  94 B1     STY B1,X
1F5C  F5 FF     SBC FF,X
1F5E  1B          ???
1F5F  71 D8     ADC (D8),Y
1F61  6B          ???
1F62  10 05     BPL 1F69
1F64  57          ???
1F65  0C          ???
1F66  BE FA A1  LDX A1FA,Y
1F69  AE 7C 69  LDX 697C
1F6C  15 30     ORA 30,X
1F6E  69 76     ADC #76
1F70  D4          ???
1F71  23          ???
1F72  77          ???
1F73  16 6A     ASL 6A,X
1F75  18        CLC 
1F76  65 49     ADC 49
1F78  3E 3E BE  ROL BE3E,X
1F7B  9B          ???
1F7C  C3          ???
1F7D  15 25     ORA 25,X
1F7F  C1 46     CMP (46,X)
1F81  60        RTS 
1F82  C4 1B     CPY 1B
1F84  F7          ???
1F85  83          ???
1F86  09 27     ORA #27
1F88  1C          ???
1F89  49 E4     EOR #E4
1F8B  D2          ???
1F8C  CC 92 88  CPY 8892
1F8F  4F          ???
1F90  0E 4D 20  ASL 204D
1F93  8F          ???
1F94  C1 E2     CMP (E2,X)
1F96  1A          ???
1F97  02          ???
1F98  66 26     ROR 26
1F9A  D2          ???
1F9B  4A        LSR A
1F9C  38        SEC 
1F9D  01 F8     ORA (F8,X)
1F9F  25 4B     AND 4B
1FA1  93          ???
1FA2  84 D8     STY D8
1FA4  24 7A     BIT 7A
1FA6  07          ???
1FA7  62          ???
1FA8  63          ???
1FA9  68        PLA 
1FAA  B8        CLV 
1FAB  3F          ???
1FAC  07          ???
1FAD  81 20     STA (20,X)
1FAF  7D 03 C9  ADC C903,X
1FB2  AD 99 A0  LDA A099
1FB5  DE 64 89  DEC 8964,X
1FB8  75 2D     ADC 2D,X
1FBA  A1 07     LDA (07,X)
1FBC  C1 27     CMP (27,X)
1FBE  BD CA 44  LDA 44CA,X
1FC1  90 34     BCC 1FF7
1FC3  6C 9B 61  JMP (619B)
1FC6  AE B4 49  LDX 49B4
1FC9  24 A9     BIT A9
1FCB  D4          ???
1FCC  6C 63 07  JMP (0763)
1FCF  09 D5     ORA #D5
1FD1  E4 96     CPX 96
1FD3  0A        ASL A
1FD4  7E 6B D3  ROR D36B,X
1FD7  77          ???
1FD8  DB          ???
1FD9  8A        TXA 
1FDA  28        PLP 
1FDB  A4 EE     LDY EE
1FDD  39 50 48  AND 4850,Y
1FE0  AC 47 94  LDY 9447
1FE3  59 BF 66  EOR 66BF,Y
1FE6  57          ???
1FE7  D2          ???
1FE8  B6 8E     LDX 8E,Y
1FEA  4D DC 17  EOR 17DC
1FED  49 0A     EOR #0A
1FEF  06 1F     ASL 1F
1FF1  09 D9     ORA #D9
1FF3  77          ???
1FF4  70 17     BVS 200D
1FF6  33          ???
1FF7  04          ???
1FF8  B9 55 5A  LDA 5A55,Y
1FFB  0B          ???
1FFC  58        CLI 
1FFD  F9 6C 1D  SBC 1D6C,Y
2000  91 E7     STA (E7),Y
2002  08        PHP 
2003  CE 61 B3  DEC B361
2006  75 56     ADC 56,X
2008  21 01     AND (01,X)
200A  06 B3     ASL B3
200C  54          ???
200D  D3          ???
200E  1D BC 1B  ORA 1BBC,X
2011  5B          ???
2012  3A          ???
2013  46 AB     LSR AB
2015  F6 EF     INC EF,X
2017  A9 58     LDA #58
2019  A7          ???
201A  21 51     AND (51,X)
201C  BF          ???
201D  31 D9     AND (D9),Y
201F  EA        NOP 
2020  DF          ???
2021  56 8B     LSR 8B,X
2023  37          ???
2024  2D BB C5  AND C5BB
2027  96 70     STX 70,Y
2029  FA          ???
202A  26 53     ROL 53
202C  57          ???
202D  01 EE     ORA (EE,X)
202F  B1 41     LDA (41),Y
2031  BD 44 85  LDA 8544,X
2034  49 B2     EOR #B2
2036  47          ???
2037  D7          ???
2038  91 40     STA (40),Y
203A  ED 2B 96  SBC 962B
203D  43          ???
203E  84 75     STY 75
2040  DE 1D 3A  DEC 3A1D,X
2043  4D 8E 7A  EOR 7A8E
2046  98        TYA 
2047  99 4A 11  STA 114A,Y
204A  B2          ???
204B  1D E8 2D  ORA 2DE8,X
204E  58        CLI 
204F  1C          ???
2050  48        PHA 
2051  BC 1F 0C  LDY 0C1F,X
2054  52          ???
2055  86 47     STX 47
2057  1A          ???
2058  C0 BC     CPY #BC
205A  3E 8D E7  ROL E78D,X
205D  85 03     STA 03
205F  39 78 FE  AND FE78,Y
2062  18        CLC 
2063  E6 22     INC 22
2065  CE 46 50  DEC 5046
2068  D8        CLD 
2069  AB          ???
206A  34          ???
206B  49 B2     EOR #B2
206D  D4          ???
206E  59 CC 1C  EOR 1CCC,Y
2071  DB          ???
2072  68        PLA 
2073  16 72     ASL 72,X
2075  D8        CLD 
2076  A9 01     LDA #01
2078  8D 49 39  STA 3949
207B  F6 27     INC 27,X
207D  E2          ???
207E  98        TYA 
207F  BA        TSX 
2080  0D F4 A8  ORA A8F4
2083  01 62     ORA (62,X)
2085  C1 EC     CMP (EC,X)
2087  FF          ???
2088  97          ???
2089  E4 54     CPX 54
208B  C2          ???
208C  A3          ???
208D  00        BRK 
208E  78        SEI 
208F  89          ???
2090  93          ???
2091  DA          ???
2092  02          ???
2093  8C 1E 81  STY 811E
2096  75 30     ADC 30,X
2098  61 73     ADC (73,X)
209A  CA        DEX 
209B  84 F7     STY F7
209D  83          ???
209E  21 89     AND (89,X)
20A0  1D 19 55  ORA 5519,X
20A3  4A        LSR A
20A4  90 4E     BCC 20F4
20A6  96 11     STX 11,Y
20A8  34          ???
20A9  AA        TAX 
20AA  12          ???
20AB  4C 3F 24  JMP 243F
20AE  CF          ???
20AF  AD 29 E8  LDA E829
20B2  29 55     AND #55
20B4  3E F0 67  ROL 67F0,X
20B7  E4 34     CPX 34
20B9  56 F1     LSR F1,X
20BB  85 89     STA 89
20BD  09 BB     ORA #BB
20BF  B5 AE     LDA AE,X
20C1  34          ???
20C2  51 FA     EOR (FA),Y
20C4  DA          ???
20C5  AB          ???
20C6  10 F6     BPL 20BE
20C8  3F          ???
20C9  77          ???
20CA  42          ???
20CB  22          ???
20CC  64          ???
20CD  46 CE     LSR CE
20CF  F0 0F     BEQ 20E0
20D1  A1 87     LDA (87,X)
20D3  34          ???
20D4  ED 0B 0D  SBC 0D0B
20D7  7C          ???
20D8  8E 16 F3  STX F316
20DB  5C          ???
20DC  6B          ???
20DD  8D 88 43  STA 4388
20E0  13          ???
20E1  93          ???
20E2  90 1C     BCC 2100
20E4  2C 79 95  BIT 9579
20E7  F4          ???
20E8  B9 90 22  LDA 2290,Y
20EB  DC          ???
20EC  37          ???
20ED  51 DA     EOR (DA),Y
20EF  25 5D     AND 5D
20F1  CD EE B7  CMP B7EE
20F4  8F          ???
20F5  A1 72     LDA (72,X)
20F7  2E 93 16  ROL 1693
20FA  4F          ???
20FB  CA        DEX 
20FC  F1 22     SBC (22),Y
20FE  E4 71     CPX 71
2100  AB          ???
2101  E0 72     CPX #72
2103  45 AA     EOR AA
2105  98        TYA 
2106  8F          ???
2107  12          ???
2108  42          ???
2109  C7          ???
210A  C5 36     CMP 36
210C  B7          ???
210D  EA        NOP 
210E  1B          ???
210F  22          ???
2110  15 DC     ORA DC,X
2112  CA        DEX 
2113  E6 C0     INC C0
2115  A6 EC     LDX EC
2117  EC E0 78  CPX 78E0
211A  93          ???
211B  63          ???
211C  F0 5E     BEQ 217C
211E  34          ???
211F  05 7B     ORA 7B
2121  4B          ???
2122  30 E2     BMI 2106
2124  FD 1F 89  SBC 891F,X
2127  26 ED     ROL ED
2129  82          ???
212A  45 F8     EOR F8
212C  10 DD     BPL 210B
212E  15 1D     ORA 1D,X
2130  98        TYA 
2131  53          ???
2132  72          ???
2133  11 05     ORA (05),Y
2135  A1 51     LDA (51,X)
2137  AE BC 24  LDX 24BC
213A  A0 C9     LDY #C9
213C  70 A0     BVS 20DE
213E  86 D3     STX D3
2140  22          ???
2141  8D 28 21  STA 2128
2144  17          ???
2145  54          ???
2146  88        DEY 
2147  AE 48 A6  LDX A648
214A  0F          ???
214B  75 8F     ADC 8F,X
214D  BE 22 08  LDX 0822,Y
2150  24 85     BIT 85
2152  15 75     ORA 75,X
2154  CE 97 0B  DEC 0B97
2157  A0 FA     LDY #FA
2159  DA          ???
215A  58        CLI 
215B  6E 4B 31  ROR 314B
215E  06 4D     ASL 4D
2160  DA          ???
2161  36 04     ROL 04,X
2163  4D 8F D0  EOR D08F
2166  F9 71 38  SBC 3871,Y
2169  50 A5     BVC 2110
216B  38        SEC 
216C  A2 98     LDX #98
216E  AB          ???
216F  BF          ???
2170  B5 F8     LDA F8,X
2172  18        CLC 
2173  C1 5B     CMP (5B,X)
2175  66 63     ROR 63
2177  77          ???
2178  67          ???
2179  05 19     ORA 19
217B  3B          ???
217C  C7          ???
217D  18        CLC 
217E  4B          ???
217F  87          ???
2180  D1 0D     CMP (0D),Y
2182  A2 CA     LDX #CA
2184  F8        SED 
2185  A1 09     LDA (09,X)
2187  D1 27     CMP (27),Y
2189  F5 17     SBC 17,X
218B  45 E1     EOR E1
218D  7E 1E 78  ROR 781E,X
2190  97          ???
2191  2A        ROL A
2192  08        PHP 
2193  5A          ???
2194  AE 87 2A  LDX 2A87
2197  31 31     AND (31),Y
2199  E6 21     INC 21
219B  FC          ???
219C  5A          ???
219D  84 10     STY 10
219F  9A        TXS 
21A0  F9 66 F9  SBC F966,Y
21A3  F7          ???
21A4  32          ???
21A5  13          ???
21A6  84 18     STY 18
21A8  6B          ???
21A9  3B          ???
21AA  37          ???
21AB  AA        TAX 
21AC  60        RTS 
21AD  DF          ???
21AE  7F          ???
21AF  67          ???
21B0  D6 42     DEC 42,X
21B2  60        RTS 
21B3  E6 9D     INC 9D
21B5  A3          ???
21B6  52          ???
21B7  C8        INY 
21B8  17          ???
21B9  C5 E6     CMP E6
21BB  39 4E 28  AND 284E,Y
21BE  DB          ???
21BF  AF          ???
21C0  01 3E     ORA (3E,X)
21C2  A5 A9     LDA A9
21C4  C2          ???
21C5  6E 9A 82  ROR 829A
21C8  8C 09 4D  STY 4D09
21CB  33          ???
21CC  53          ???
21CD  25 72     AND 72
21CF  EB          ???
21D0  C7          ???
21D1  13          ???
21D2  01 10     ORA (10,X)
21D4  76 EC     ROR EC,X
21D6  9E          ???
21D7  0D C6 D4  ORA D4C6
21DA  89          ???
21DB  CD E3 86  CMP 86E3
21DE  DD 3A 07  CMP 073A,X
21E1  1A          ???
21E2  D9 2A 48  CMP 482A,Y
21E5  08        PHP 
21E6  51 F5     EOR (F5),Y
21E8  6C 12 AD  JMP (AD12)
21EB  1D 52 A9  ORA A952,X
21EE  83          ???
21EF  8A        TXA 
21F0  C6 8D     DEC 8D
21F2  04          ???
21F3  25 73     AND 73
21F5  08        PHP 
21F6  2C 13 A0  BIT A013
21F9  A5 9C     LDA 9C
21FB  C0 6A     CPY #6A
21FD  5D 95 08  EOR 0895,X
2200  50 83     BVC 2185
2202  38        SEC 
2203  C4 F4     CPY F4
2205  39 3B 15  AND 153B,Y
2208  34          ???
2209  38        SEC 
220A  01 34     ORA (34,X)
220C  0D 19 1C  ORA 1C19
220F  0D 92 55  ORA 5592
2212  CE D5 D2  DEC D2D5
2215  6D 0B EB  ADC EB0B
2218  B5 E5     LDA E5,X
221A  76 E1     ROR E1,X
221C  DB          ???
221D  95 80     STA 80,X
221F  21 1E     AND (1E,X)
2221  30 AE     BMI 21D1
2223  8D CE 47  STA 47CE
2226  27          ???
2227  32          ???
2228  34          ???
2229  78        SEI 
222A  EE 23 CD  INC CD23
222D  82          ???
222E  E4 08     CPX 08
2230  F3          ???
2231  09 10     ORA #10
2233  A7          ???
2234  73          ???
2235  4C 3C 66  JMP 663C
2238  E1 70     SBC (70,X)
223A  E3          ???
223B  BD 3D 6F  LDA 6F3D,X
223E  DC          ???
223F  9A        TXS 
2240  6D 87 84  ADC 8487
2243  5D 74 AA  EOR AA74,X
2246  3A          ???
2247  0D 39 37  ORA 3739
224A  01 1B     ORA (1B,X)
224C  BA        TSX 
224D  26 8C     ROL 8C
224F  3C          ???
2250  4F          ???
2251  A7          ???
2252  89          ???
2253  47          ???
2254  7E 8D F8  ROR F88D,X
2257  1A          ???
2258  34          ???
2259  08        PHP 
225A  DB          ???
225B  F5 B7     SBC B7,X
225D  94 BE     STY BE,X
225F  FA          ???
2260  ED CA 01  SBC 01CA
2263  BB          ???
2264  06 0C     ASL 0C
2266  0F          ???
2267  E0 58     CPX #58
2269  A9 32     LDA #32
226B  18        CLC 
226C  57          ???
226D  31 09     AND (09),Y
226F  13          ???
2270  10 27     BPL 2299
2272  C2          ???
2273  A9 31     LDA #31
2275  70 B9     BVS 2230
2277  0F          ???
2278  94 6C     STY 6C,X
227A  A0 50     LDY #50
227C  98        TYA 
227D  68        PLA 
227E  CC 75 52  CPY 5275
2281  B9 A4 72  LDA 72A4,Y
2284  59 6D 30  EOR 306D,Y
2287  B2          ???
2288  6E 60 74  ROR 7460
228B  3A          ???
228C  6F          ???
228D  82          ???
228E  F1 10     SBC (10),Y
2290  D3          ???
2291  D6 F6     DEC F6,X
2293  43          ???
2294  2D 70 1D  AND 1D70
2297  26 18     ROL 18
2299  26 1D     ROL 1D
229B  57          ???
229C  D7          ???
229D  1F          ???
229E  53          ???
229F  5A          ???
22A0  EA        NOP 
22A1  4D 35 32  EOR 3235
22A4  B2          ???
22A5  3A          ???
22A6  DC          ???
22A7  A3          ???
22A8  58        CLI 
22A9  8E DA 2C  STX 2CDA
22AC  8D EC 20  STA 20EC
22AF  12          ???
22B0  11 6D     ORA (6D),Y
22B2  03          ???
22B3  57          ???
22B4  5F          ???
22B5  46 2A     LSR 2A
22B7  0C          ???
22B8  13          ???
22B9  32          ???
22BA  05 6F     ORA 6F
22BC  B4 DC     LDY DC,X
22BE  86 C9     STX C9
22C0  12          ???
22C1  EF          ???
22C2  BC 46 C5  LDY C546,X
22C5  7C          ???
22C6  D4          ???
22C7  4C C8 74  JMP 74C8
22CA  44          ???
22CB  B6 C1     LDX C1,Y
22CD  35 99     AND 99,X
22CF  53          ???
22D0  16 12     ASL 12,X
22D2  12          ???
22D3  D9 83 99  CMP 9983,Y
22D6  20 E1 BF  JSR BFE1
22D9  12          ???
22DA  1C          ???
22DB  1C          ???
22DC  35 23     AND 23,X
22DE  4F          ???
22DF  7E 13 E6  ROR E613,X
22E2  D3          ???
22E3  A4 51     LDY 51
22E5  B8        CLV 
22E6  A5 4F     LDA 4F
22E8  53          ???
22E9  A9 02     LDA #02
22EB  9A        TXS 
22EC  0F          ???
22ED  58        CLI 
22EE  1E 4C 27  ASL 274C,X
22F1  E4 56     CPX 56
22F3  0E FB 67  ASL 67FB
22F6  18        CLC 
22F7  7D 47 4A  ADC 4A47,X
22FA  CC 98 81  CPY 8198
22FD  71 9A     ADC (9A),Y
22FF  50 64     BVC 2365
2301  61 2F     ADC (2F,X)
2303  35 B9     AND B9,X
2305  2F          ???
2306  89          ???
2307  59 60 1D  EOR 1D60,Y
230A  01 5B     ORA (5B,X)
230C  B1 94     LDA (94),Y
230E  32          ???
230F  B4 1A     LDY 1A,X
2311  C3          ???
2312  C6 22     DEC 22
2314  0D 6E 18  ORA 186E
2317  15 20     ORA 20,X
2319  EA        NOP 
231A  1A          ???
231B  4E 0B 22  LSR 220B
231E  4F          ???
231F  C5 0A     CMP 0A
2321  F7          ???
2322  6B          ???
2323  58        CLI 
2324  8D A9 20  STA 20A9
2327  8E 37 88  STX 8837
232A  7E 9C 02  ROR 029C,X
232D  38        SEC 
232E  AA        TAX 
232F  3D 06 8E  AND 8E06,X
2332  F8        SED 
2333  79 41 DC  ADC DC41,Y
2336  9E          ???
2337  9E          ???
2338  76 3B     ROR 3B,X
233A  48        PHA 
233B  DF          ???
233C  2B          ???
233D  A5 61     LDA 61
233F  A1 19     LDA (19,X)
2341  08        PHP 
2342  54          ???
2343  B1 D4     LDA (D4),Y
2345  F8        SED 
2346  24 B1     BIT B1
2348  26 0C     ROL 0C
234A  B2          ???
234B  06 01     ASL 01
234D  3E 08 22  ROL 2208,X
2350  4D 60 14  EOR 1460
2353  84 DF     STY DF
2355  1E 05 97  ASL 9705,X
2358  07          ???
2359  B1 DC     LDA (DC),Y
235B  EC D6 9E  CPX 9ED6
235E  DA          ???
235F  36 4C     ROL 4C,X
2361  DF          ???
2362  9A        TXS 
2363  42          ???
2364  44          ???
2365  DF          ???
2366  38        SEC 
2367  8C F5 98  STY 98F5
236A  FC          ???
236B  0B          ???
236C  76 E5     ROR E5,X
236E  5B          ???
236F  81 FD     STA (FD,X)
2371  0D 2E 0C  ORA 0C2E
2374  39 C5 D0  AND D0C5,Y
2377  27          ???
2378  D5 C7     CMP C7,X
237A  68        PLA 
237B  17          ???
237C  6F          ???
237D  E5 2E     SBC 2E
237F  CE 21 D6  DEC D621
2382  55 D0     EOR D0,X
2384  8D FC DB  STA DBFC
2387  71 8E     ADC (8E),Y
2389  55 8B     EOR 8B,X
238B  3C          ???
238C  C9 3B     CMP #3B
238E  6A        ROR A
238F  5F          ???
2390  1B          ???
2391  CE 47 04  DEC 0447
2394  A6 0A     LDX 0A
2396  64          ???
2397  4D 15 D9  EOR D915
239A  08        PHP 
239B  F1 31     SBC (31),Y
239D  EB          ???
239E  F1 DF     SBC (DF),Y
23A0  4D 1B A0  EOR A01B
23A3  F5 63     SBC 63,X
23A5  51 CC     EOR (CC),Y
23A7  87          ???
23A8  0E 51 75  ASL 7551
23AB  83          ???
23AC  20 F6 21  JSR 21F6
23AF  75 7C     ADC 7C,X
23B1  15 99     ORA 99,X
23B3  F2          ???
23B4  10 61     BPL 2417
23B6  C5 CE     CMP CE
23B8  FC          ???
23B9  62          ???
23BA  64          ???
23BB  5E AD 9D  LSR 9DAD,X
23BE  8E 5A AB  STX AB5A
23C1  E4 21     CPX 21
23C3  26 6B     ROL 6B
23C5  4E 4B B6  LSR B64B
23C8  2D 22 9F  AND 9F22
23CB  40        RTI 
23CC  26 62     ROL 62
23CE  61 B6     ADC (B6,X)
23D0  04          ???
23D1  6D B3 1A  ADC 1AB3
23D4  25 03     AND 03
23D6  1A          ???
23D7  FB          ???
23D8  96 D6     STX D6,Y
23DA  74          ???
23DB  D5 1A     CMP 1A,X
23DD  A3          ???
23DE  20 D4 0E  JSR 0ED4
23E1  6E 54 ED  ROR ED54
23E4  58        CLI 
23E5  0B          ???
23E6  B9 14 0C  LDA 0C14,Y
23E9  DB          ???
23EA  BD 79 00  LDA 0079,X
23ED  88        DEY 
23EE  04          ???
23EF  C1 9B     CMP (9B,X)
23F1  70 43     BVS 2436
23F3  8C D2 9A  STY 9AD2
23F6  B9 34 9F  LDA 9F34,Y
23F9  12          ???
23FA  36 5E     ROL 5E,X
23FC  6C E5 78  JMP (78E5)
23FF  49 0B     EOR #0B
2401  58        CLI 
2402  13          ???
2403  66 B8     ROR B8
2405  5B          ???
2406  CB          ???
2407  5E 45 DB  LSR DB45,X
240A  98        TYA 
240B  64          ???
240C  56 90     LSR 90,X
240E  40        RTI 
240F  7A          ???
2410  1D 52 66  ORA 6652,X
2413  79 51 5A  ADC 5A51,Y
2416  2C 7F CD  BIT CD7F
2419  0A        ASL A
241A  1B          ???
241B  7D 53 B9  ADC B953,X
241E  C9 10     CMP #10
2420  E4 15     CPX 15
2422  BE B3 BB  LDX BBB3,Y
2425  92          ???
2426  54          ???
2427  E2          ???
2428  21 58     AND (58,X)
242A  18        CLC 
242B  C3          ???
242C  10 E0     BPL 240E
242E  89          ???
242F  BE 32 E7  LDX E732,Y
2432  6F          ???
2433  0A        ASL A
2434  E3          ???
2435  43          ???
2436  35 C9     AND C9,X
2438  75 C0     ADC C0,X
243A  C5 4E     CMP 4E
243C  8D B8 C4  STA C4B8
243F  F0 B1     BEQ 23F2
2441  8C 2E 2D  STY 2D2E
2444  90 3B     BCC 2481
2446  07          ???
2447  35 2B     AND 2B,X
2449  9F          ???
244A  96 4C     STX 4C,Y
244C  D1 35     CMP (35),Y
244E  C0 8B     CPY #8B
2450  E5 C2     SBC C2
2452  5D 06 0D  EOR 0D06,X
2455  3F          ???
2456  57          ???
2457  57          ???
2458  B3          ???
2459  61 0A     ADC (0A,X)
245B  D0 28     BNE 2485
245D  8D 0E 3F  STA 3F0E
2460  0C          ???
2461  AB          ???
2462  0F          ???
2463  9B          ???
2464  C0 9B     CPY #9B
2466  53          ???
2467  09 A4     ORA #A4
2469  6D 3A CD  ADC CD3A
246C  CE C7 1B  DEC 1BC7
246F  D4          ???
2470  53          ???
2471  CB          ???
2472  C5 0D     CMP 0D
2474  32          ???
2475  87          ???
2476  07          ???
2477  28        PLP 
2478  60        RTS 
2479  13          ???
247A  03          ???
247B  71 AA     ADC (AA),Y
247D  4B          ???
247E  85 33     STA 33
2480  38        SEC 
2481  35 0C     AND 0C,X
2483  E8        INX 
2484  4E AC 1A  LSR 1AAC
2487  C4 17     CPY 17
2489  9B          ???
248A  95 78     STA 78,X
248C  71 E7     ADC (E7),Y
248E  42          ???
248F  AD 1E 69  LDA 691E
2492  EB          ???
2493  DE 3E 86  DEC 863E,X
2496  19 CF 3D  ORA 3DCF,Y
2499  66 1B     ROR 1B
249B  63          ???
249C  88        DEY 
249D  72          ???
249E  10 92     BPL 2432
24A0  C9 84     CMP #84
24A2  F0 76     BEQ 251A
24A4  EA        NOP 
24A5  78        SEI 
24A6  12          ???
24A7  60        RTS 
24A8  1B          ???
24A9  01 29     ORA (29,X)
24AB  13          ???
24AC  63          ???
24AD  CC 8E 5D  CPY 5D8E
24B0  CD F0 2C  CMP 2CF0
24B3  F1 74     SBC (74),Y
24B5  CF          ???
24B6  09 37     ORA #37
24B8  50 86     BVC 2440
24BA  3D 90 26  AND 2690,X
24BD  DA          ???
24BE  8A        TXA 
24BF  9B          ???
24C0  C4 6A     CPY 6A
24C2  B0 8E     BCS 2452
24C4  43          ???
24C5  F3          ???
24C6  B7          ???
24C7  6A        ROR A
24C8  1B          ???
24C9  D9 F2 47  CMP 47F2,Y
24CC  03          ???
24CD  3A          ???
24CE  06 41     ASL 41
24D0  3C          ???
24D1  DE 28 F9  DEC F928,X
24D4  DE 19 97  DEC 9719,X
24D7  4B          ???
24D8  CE 21 1A  DEC 1A21
24DB  1A          ???
24DC  7A          ???
24DD  E8        INX 
24DE  C7          ???
24DF  B7          ???
24E0  1B          ???
24E1  1C          ???
24E2  50 66     BVC 254A
24E4  28        PLP 
24E5  7E 74 5E  ROR 5E74,X
24E8  F6 19     INC 19,X
24EA  64          ???
24EB  6E 9B 85  ROR 859B
24EE  CC 8D FF  CPY FF8D
24F1  61 F9     ADC (F9,X)
24F3  9A        TXS 
24F4  88        DEY 
24F5  DF          ???
24F6  42          ???
24F7  B9 8E 51  LDA 518E,Y
24FA  1C          ???
24FB  41 5E     EOR (5E,X)
24FD  3C          ???
24FE  3B          ???
24FF  2D 11 18  AND 1811
2502  DB          ???
2503  96 12     STX 12,Y
2505  B3          ???
2506  6D 45 D1  ADC D145
2509  31 D0     AND (D0),Y
250B  32          ???
250C  5E 6F 4F  LSR 4F6F,X
250F  6D 16 12  ADC 1216
2512  2E 1F 3C  ROL 3C1F
2515  CC 13 DB  CPY DB13
2518  18        CLC 
2519  7C          ???
251A  AE A5 8D  LDX 8DA5
251D  70 02     BVS 2521
251F  77          ???
2520  32          ???
2521  22          ???
2522  A7          ???
2523  19 0D C7  ORA C70D,Y
2526  77          ???
2527  32          ???
2528  22          ???
2529  B0 D4     BCS 24FF
252B  2E 93 47  ROL 4793
252E  BA        TSX 
252F  10 E3     BPL 2514
2531  0A        ASL A
2532  84 4E     STY 4E
2534  67          ???
2535  C4 74     CPY 74
2537  05 1E     ORA 1E
2539  1A          ???
253A  06 F2     ASL F2
253C  45 71     EOR 71
253E  4F          ???
253F  C6 80     DEC 80
2541  49 B2     EOR #B2
2543  3E DA 6D  ROL 6DDA,X
2546  28        PLP 
2547  29 59     AND #59
2549  CA        DEX 
254A  EB          ???
254B  F8        SED 
254C  54          ???
254D  38        SEC 
254E  27          ???
254F  8E 33 65  STX 6533
2552  8D 49 0C  STA 0C49
2555  D2          ???
2556  96 04     STX 04,Y
2558  D7          ???
2559  49 8B     EOR #8B
255B  85 9C     STA 9C
255D  9E          ???
255E  46 A1     LSR A1
2560  A1 1A     LDA (1A,X)
2562  2D 4F AF  AND AF4F
2565  B2          ???
2566  2C 34 00  BIT 0034
2569  12          ???
256A  92          ???
256B  B5 EE     LDA EE,X
256D  F5 F7     SBC F7,X
256F  45 B0     EOR B0
2571  CA        DEX 
2572  B6 9C     LDX 9C,Y
2574  48        PHA 
2575  EF          ???
2576  E7          ???
2577  9A        TXS 
2578  6E 0E C9  ROR C90E
257B  3A          ???
257C  57          ???
257D  8D D5 74  STA 74D5
2580  E0 80     CPX #80
2582  B0 A5     BCS 2529
2584  07          ???
2585  C8        INY 
2586  EF          ???
2587  A6 3C     LDX 3C
2589  F3          ???
258A  71 B8     ADC (B8),Y
258C  E3          ???
258D  3E 16 98  ROL 9816,X
2590  C6 1F     DEC 1F
2592  8A        TXA 
2593  12          ???
2594  9A        TXS 
2595  A1 A9     LDA (A9,X)
2597  1F          ???
2598  79 02 D4  ADC D402,Y
259B  33          ???
259C  84 34     STY 34
259E  B7          ???
259F  B3          ???
25A0  4C 3A AA  JMP AA3A
25A3  4C 2A F1  JMP F12A
25A6  12          ???
25A7  6A        ROR A
25A8  50 93     BVC 253D
25AA  D8        CLD 
25AB  E7          ???
25AC  37          ???
25AD  45 55     EOR 55
25AF  80          ???
25B0  DF          ???
25B1  B1 13     LDA (13),Y
25B3  13          ???
25B4  20 90 2D  JSR 2D90
25B7  5F          ???
25B8  8C 0E B7  STY B70E
25BB  9E          ???
25BC  5F          ???
25BD  0D 94 F0  ORA F094
25C0  2D 01 02  AND 0201
25C3  1C          ???
25C4  27          ???
25C5  3D 73 6B  AND 6B73,X
25C8  BD 75 48  LDA 4875,X
25CB  BE 63 92  LDX 9263,Y
25CE  60        RTS 
25CF  97          ???
25D0  5A          ???
25D1  2A        ROL A
25D2  30 DE     BMI 25B2
25D4  A4 07     LDY 07
25D6  42          ???
25D7  63          ???
25D8  7D 57 AB  ADC AB57,X
25DB  50 05     BVC 25E2
25DD  A4 4E     LDY 4E
25DF  28        PLP 
25E0  24 19     BIT 19
25E2  24 35     BIT 35
25E4  A7          ???
25E5  82          ???
25E6  19 0B A5  ORA A50B,Y
25E9  80          ???
25EA  B5 47     LDA 47,X
25EC  B2          ???
25ED  E8        INX 
25EE  5D 4C 72  EOR 724C,X
25F1  4E 14 83  LSR 8314
25F4  28        PLP 
25F5  1E DB A7  ASL A7DB,X
25F8  C3          ???
25F9  61 B9     ADC (B9,X)
25FB  85 F0     STA F0
25FD  D5 0F     CMP 0F,X
25FF  8D 6C 73  STA 736C
2602  40        RTI 
2603  C9 E6     CMP #E6
2605  45 4A     EOR 4A
2607  E1 C2     SBC (C2,X)
2609  F0 3C     BEQ 2647
260B  32          ???
260C  86 DA     STX DA
260E  2C 12 FA  BIT FA12
2611  A7          ???
2612  92          ???
2613  94 94     STY 94,X
2615  83          ???
2616  B2          ???
2617  1E 0D C0  ASL C00D,X
261A  8C DE EA  STY EADE
261D  37          ???
261E  78        SEI 
261F  D4          ???
2620  20 54 12  JSR 1254
2623  05 2E     ORA 2E
2625  A0 24     LDY #24
2627  97          ???
2628  B5 90     LDA 90,X
262A  67          ???
262B  2F          ???
262C  1B          ???
262D  67          ???
262E  58        CLI 
262F  B5 24     LDA 24,X
2631  0F          ???
2632  4D 86 42  EOR 4286
2635  E6 BA     INC BA
2637  3D 4B E2  AND E24B,X
263A  B0 B4     BCS 25F0
263C  0F          ???
263D  91 CC     STA (CC),Y
263F  9F          ???
2640  E9 ED     SBC #ED
2642  94 2B     STY 2B,X
2644  6A        ROR A
2645  59 E7 02  EOR 02E7,Y
2648  66 13     ROR 13
264A  54          ???
264B  69 02     ADC #02
264D  AC C6 1C  LDY 1CC6
2650  E1 87     SBC (87,X)
2652  8E 5B EB  STX EB5B
2655  9E          ???
2656  71 74     ADC (74),Y
2658  9F          ???
2659  94 20     STY 20,X
265B  4D 07 70  EOR 7007
265E  9A        TXS 
265F  A6 CB     LDX CB
2661  8F          ???
2662  66 72     ROR 72
2664  62          ???
2665  E3          ???
2666  8D C9 14  STA 14C9
2669  47          ???
266A  89          ???
266B  1F          ???
266C  7B          ???
266D  EA        NOP 
266E  36 5A     ROL 5A,X
2670  A8        TAY 
2671  59 53 42  EOR 4253,Y
2674  F4          ???
2675  68        PLA 
2676  BC 9F 98  LDY 989F,X
2679  8D A7 E5  STA E5A7
267C  37          ???
267D  02          ???
267E  84 91     STY 91
2680  85 20     STA 20
2682  19 B6 C6  ORA C6B6,Y
2685  83          ???
2686  B8        CLV 
2687  47          ???
2688  2E 15 44  ROL 4415
268B  D5 18     CMP 18,X
268D  68        PLA 
268E  6B          ???
268F  3E 7E 75  ROL 757E,X
2692  B2          ???
2693  B7          ???
2694  6B          ???
2695  17          ???
2696  AA        TAX 
2697  E1 F7     SBC (F7,X)
2699  0A        ASL A
269A  37          ???
269B  A5 89     LDA 89
269D  31 9D     AND (9D),Y
269F  34          ???
26A0  32          ???
26A1  39 2E F8  AND F82E,Y
26A4  6D 71 86  ADC 8671
26A7  AB          ???
26A8  5D C1 0A  EOR 0AC1,X
26AB  E7          ???
26AC  FC          ???
26AD  EA        NOP 
26AE  55 B6     EOR B6,X
26B0  6F          ???
26B1  B9 7E A1  LDA A17E,Y
26B4  F6 1A     INC 1A,X
26B6  15 48     ORA 48,X
26B8  6A        ROR A
26B9  99 CD 13  STA 13CD,Y
26BC  6D 16 D6  ADC D616
26BF  49 83     EOR #83
26C1  06 A3     ASL A3
26C3  59 EE D3  EOR D3EE,Y
26C6  33          ???
26C7  A8        TAY 
26C8  7C          ???
26C9  1D 04 6A  ORA 6A04,X
26CC  3E 0C 2E  ROL 2E0C,X
26CF  AE D3 A5  LDX A5D3
26D2  99 BB 90  STA 90BB,Y
26D5  9F          ???
26D6  1B          ???
26D7  46 7C     LSR 7C
26D9  8A        TXA 
26DA  3B          ???
26DB  0D B8 2C  ORA 2CB8
26DE  66 CC     ROR CC
26E0  2C 96 BB  BIT BB96
26E3  E2          ???
26E4  7A          ???
26E5  E4 57     CPX 57
26E7  31 C4     AND (C4),Y
26E9  7C          ???
26EA  32          ???
26EB  BE A9 DA  LDX DAA9,Y
26EE  28        PLP 
26EF  32          ???
26F0  4C BA 52  JMP 52BA
26F3  13          ???
26F4  ED 08 19  SBC 1908
26F7  29 EB     AND #EB
26F9  1E 26 8D  ASL 8D26,X
26FC  07          ???
26FD  C9 57     CMP #57
26FF  BA        TSX 
2700  D2          ???
2701  CF          ???
2702  1E 57 C6  ASL C657,X
2705  B8        CLV 
2706  22          ???
2707  1D C8 D9  ORA D9C8,X
270A  50 3A     BVC 2746
270C  A6 4E     LDX 4E
270E  8B          ???
270F  01 C5     ORA (C5,X)
2711  01 DB     ORA (DB,X)
2713  9C          ???
2714  11 A2     ORA (A2),Y
2716  D9 78 A0  CMP A078,Y
2719  D6 32     DEC 32,X
271B  D1 31     CMP (31),Y
271D  BF          ???
271E  1D 92 0F  ORA 0F92,X
2721  8F          ???
2722  86 C6     STX C6
2724  4A        LSR A
2725  78        SEI 
2726  CE E8 B7  DEC B7E8
2729  1D B4 AB  ORA ABB4,X
272C  BF          ???
272D  A5 8B     LDA 8B
272F  F8        SED 
2730  0B          ???
2731  D3          ???
2732  28        PLP 
2733  6B          ???
2734  49 BB     EOR #BB
2736  EC 5F 0E  CPX 0E5F
2739  13          ???
273A  3F          ???
273B  D3          ???
273C  C1 33     CMP (33,X)
273E  3C          ???
273F  65 AF     ADC AF
2741  2C 04 94  BIT 9404
2744  B8        CLV 
2745  C5 50     CMP 50
2747  0D FD 36  ORA 36FD
274A  87          ???
274B  C6 36     DEC 36
274D  A1 49     LDA (49,X)
274F  BB          ???
2750  C5 D2     CMP D2
2752  F9 C9 E0  SBC E0C9,Y
2755  AB          ???
2756  15 26     ORA 26,X
2758  89          ???
2759  36 D0     ROL D0,X
275B  09 01     ORA #01
275D  1B          ???
275E  43          ???
275F  F5 3D     SBC 3D,X
2761  8F          ???
2762  4D 69 4C  EOR 4C69
2765  58        CLI 
2766  2E 2D 91  ROL 912D
2769  EB          ???
276A  6C 00 9E  JMP (9E00)
276D  D4          ???
276E  09 2F     ORA #2F
2770  8E 62 8E  STX 8E62
2773  2D 05 22  AND 2205
2776  BC 4F 56  LDY 564F,X
2779  49 3C     EOR #3C
277B  EE BE 18  INC 18BE
277E  40        RTI 
277F  E1 7C     SBC (7C,X)
2781  DC          ???
2782  A5 8D     LDA 8D
2784  6E 68 F0  ROR F068
2787  F4          ???
2788  DA          ???
2789  53          ???
278A  A7          ???
278B  C0 A1     CPY #A1
278D  A7          ???
278E  21 7A     AND (7A,X)
2790  78        SEI 
2791  48        PHA 
2792  B7          ???
2793  4E D2 5D  LSR 5DD2
2796  43          ???
2797  E0 1A     CPX #1A
2799  67          ???
279A  0D 0C C6  ORA C60C
279D  6C 78 13  JMP (1378)
27A0  32          ???
27A1  7E E0 7A  ROR 7AE0,X
27A4  CD 15 A4  CMP A415
27A7  D7          ???
27A8  70 A9     BVS 2753
27AA  13          ???
27AB  21 FE     AND (FE,X)
27AD  24 2C     BIT 2C
27AF  B6 9A     LDX 9A,Y
27B1  47          ???
27B2  7F          ???
27B3  D4          ???
27B4  08        PHP 
27B5  7B          ???
27B6  2C 89 8B  BIT 8B89
27B9  BE 00 13  LDX 1300,Y
27BC  47          ???
27BD  C0 C3     CPY #C3
27BF  66 D7     ROR D7
27C1  38        SEC 
27C2  99 A9 E9  STA E9A9,Y
27C5  B3          ???
27C6  CB          ???
27C7  01 A6     ORA (A6,X)
27C9  26 48     ROL 48
27CB  A1 1B     LDA (1B,X)
27CD  79 89 16  ADC 1689,Y
27D0  1D 13 5D  ORA 5D13,X
27D3  04          ???
27D4  07          ???
27D5  B7          ???
27D6  D0 1E     BNE 27F6
27D8  50 84     BVC 275E
27DA  A0 91     LDY #91
27DC  76 54     ROR 54,X
27DE  77          ???
27DF  52          ???
27E0  BA        TSX 
27E1  80          ???
27E2  DC          ???
27E3  BB          ???
27E4  96 47     STX 47,Y
27E6  B9 E5 1D  LDA 1DE5,Y
27E9  31 1D     AND (1D),Y
27EB  D0 5A     BNE 2847
27ED  3E AF 79  ROL 79AF,X
27F0  02          ???
27F1  83          ???
27F2  4A        LSR A
27F3  1E 6A B6  ASL B66A,X
27F6  0B          ???
27F7  8C 1E 93  STY 931E
27FA  97          ???
27FB  01 C5     ORA (C5,X)
27FD  F5 16     SBC 16,X
27FF  56 8B     LSR 8B,X
2801  84 44     STY 44
2803  FA          ???
2804  E7          ???
2805  87          ???
2806  51 8D     EOR (8D),Y
2808  A3          ???
2809  44          ???
280A  B4 A1     LDY A1,X
280C  7E BC 1F  ROR 1FBC,X
280F  0F          ???
2810  80          ???
2811  39 6B C6  AND C66B,Y
2814  8D 45 23  STA 2345
2817  30 B8     BMI 27D1
2819  30 36     BMI 2851
281B  97          ???
281C  D9 B2 6C  CMP 6CB2,Y
281F  01 D5     ORA (D5,X)
2821  6E 1A EE  ROR EE1A
2824  B4 E9     LDY E9,X
2826  24 3A     BIT 3A
2828  5F          ???
2829  4B          ???
282A  6B          ???
282B  D4          ???
282C  31 39     AND (39),Y
282E  60        RTS 
282F  32          ???
2830  8D BB 91  STA 91BB
2833  70 31     BVS 2866
2835  44          ???
2836  05 53     ORA 53
2838  38        SEC 
2839  CB          ???
283A  BD F1 F8  LDA F8F1,X
283D  64          ???
283E  56 BA     LSR BA,X
2840  10 53     BPL 2895
2842  DB          ???
2843  48        PHA 
2844  54          ???
2845  22          ???
2846  6C 17 9B  JMP (9B17)
2849  17          ???
284A  B1 A9     LDA (A9),Y
284C  F2          ???
284D  A5 98     LDA 98
284F  E5 29     SBC 29
2851  6A        ROR A
2852  DD ED 1C  CMP 1CED,X
2855  67          ???
2856  13          ???
2857  E5 C0     SBC C0
2859  CE 74 53  DEC 5374
285C  57          ???
285D  1C          ???
285E  A8        TAY 
285F  14          ???
2860  68        PLA 
2861  D3          ???
2862  5E DB B0  LSR B0DB,X
2865  E2          ???
2866  48        PHA 
2867  E0 7A     CPX #7A
2869  EC 40 A4  CPX A440
286C  13          ???
286D  D4          ???
286E  DD 43 94  CMP 9443,X
2871  C5 F3     CMP F3
2873  33          ???
2874  35 22     AND 22,X
2876  B4 89     LDY 89,X
2878  2E B6 7E  ROL 7EB6
287B  EF          ???
287C  87          ???
287D  68        PLA 
287E  9F          ???
287F  58        CLI 
2880  19 5A F8  ORA F85A,Y
2883  97          ???
2884  2E AC E0  ROL E0AC
2887  B5 43     LDA 43,X
2889  02          ???
288A  54          ???
288B  47          ???
288C  29 4D     AND #4D
288E  15 3F     ORA 3F,X
2890  1F          ???
2891  7C          ???
2892  09 CA     ORA #CA
2894  DC          ???
2895  8C AC 03  STY 03AC
2898  67          ???
2899  90 BD     BCC 2858
289B  5A          ???
289C  A5 10     LDA 10
289E  29 33     AND #33
28A0  89          ???
28A1  37          ???
28A2  7D A8 07  ADC 07A8,X
28A5  4B          ???
28A6  78        SEI 
28A7  52          ???
28A8  91 E3     STA (E3),Y
28AA  4F          ???
28AB  CA        DEX 
28AC  08        PHP 
28AD  8E B7 BD  STX BDB7
28B0  94 19     STY 19,X
28B2  73          ???
28B3  31 34     AND (34),Y
28B5  03          ???
28B6  EA        NOP 
28B7  2C 76 82  BIT 8276
28BA  97          ???
28BB  07          ???
28BC  62          ???
28BD  E6 24     INC 24
28BF  31 9E     AND (9E),Y
28C1  34          ???
28C2  2E 00 C0  ROL C000
28C5  04          ???
28C6  EC 74 EF  CPX EF74
28C9  2D 53 02  AND 0253
28CC  25 F1     AND F1
28CE  EE 67 FB  INC FB67
28D1  6A        ROR A
28D2  19 62 7A  ORA 7A62,Y
28D5  9B          ???
28D6  C9 8D     CMP #8D
28D8  0C          ???
28D9  DF          ???
28DA  86 F2     STX F2
28DC  6F          ???
28DD  99 8B 16  STA 168B,Y
28E0  3D B2 A6  AND A6B2,X
28E3  E4 62     CPX 62
28E5  0F          ???
28E6  0E 52 A6  ASL A652
28E9  75 2E     ADC 2E,X
28EB  16 11     ASL 11,X
28ED  97          ???
28EE  7D 5B 03  ADC 035B,X
28F1  8C CD 93  STY 93CD
28F4  68        PLA 
28F5  5C          ???
28F6  69 21     ADC #21
28F8  11 F2     ORA (F2),Y
28FA  D6 6E     DEC 6E,X
28FC  50 41     BVC 293F
28FE  AD A8 B5  LDA B5A8
2901  34          ???
2902  53          ???
2903  58        CLI 
2904  D9 39 D1  CMP D139,Y
2907  89          ???
2908  B0 EA     BCS 28F4
290A  4E 16 87  LSR 8716
290D  2D 9D 85  AND 859D
2910  C7          ???
2911  A8        TAY 
2912  01 17     ORA (17,X)
2914  A4 D5     LDY D5
2916  70 14     BVS 292C
2918  FA          ???
2919  3E 16 13  ROL 1316,X
291C  E5 C5     SBC C5
291E  B4 7C     LDY 7C,X
2920  06 D3     ASL D3
2922  8B          ???
2923  CA        DEX 
2924  B3          ???
2925  AA        TAX 
2926  45 64     EOR 64
2928  6F          ???
2929  78        SEI 
292A  08        PHP 
292B  20 A3 2A  JSR 2AA3
292E  53          ???
292F  11 9B     ORA (9B),Y
2931  40        RTI 
2932  C5 03     CMP 03
2934  BB          ???
2935  83          ???
2936  6D 12 6D  ADC 6D12
2939  1C          ???
293A  E7          ???
293B  E9 78     SBC #78
293D  57          ???
293E  D9 BC 35  CMP 35BC,Y
2941  F1 13     SBC (13),Y
2943  2A        ROL A
2944  0B          ???
2945  91 49     STA (49),Y
2947  2C CD BD  BIT BDCD
294A  F7          ???
294B  05 63     ORA 63
294D  7E BF 21  ROR 21BF,X
2950  F9 C0 9B  SBC 9BC0,Y
2953  15 12     ORA 12,X
2955  1D 96 18  ORA 1896,X
2958  84 97     STY 97
295A  CD 49 F5  CMP F549
295D  89          ???
295E  8D 5E F2  STA F25E
2961  BF          ???
2962  D0 31     BNE 2995
2964  23          ???
2965  3B          ???
2966  58        CLI 
2967  9B          ???
2968  E3          ???
2969  54          ???
296A  A7          ???
296B  A9 A9     LDA #A9
296D  AA        TAX 
296E  AF          ???
296F  2C DA D4  BIT D4DA
2972  F2          ???
2973  8A        TXA 
2974  B3          ???
2975  40        RTI 
2976  47          ???
2977  5A          ???
2978  F1 2C     SBC (2C),Y
297A  32          ???
297B  0F          ???
297C  BD B1 46  LDA 46B1,X
297F  EE 25 4D  INC 4D25
2982  76 D9     ROR D9,X
2984  0B          ???
2985  BB          ???
2986  A3          ???
2987  0F          ???
2988  56 41     LSR 41,X
298A  B4 4E     LDY 4E,X
298C  4E 0B 5C  LSR 5C0B
298F  E5 69     SBC 69
2991  BC 49 A5  LDY A549,X
2994  17          ???
2995  7B          ???
2996  BD 5F 51  LDA 515F,X
2999  19 F1 8F  ORA 8FF1,Y
299C  2C 13 41  BIT 4113
299F  A3          ???
29A0  21 D9     AND (D9,X)
29A2  4D 3C 08  EOR 083C
29A5  6C 04 51  JMP (5104)
29A8  8F          ???
29A9  76 12     ROR 12,X
29AB  A8        TAY 
29AC  A4 D4     LDY D4
29AE  6E 40 1E  ROR 1E40
29B1  DA          ???
29B2  12          ???
29B3  20 81 C3  JSR C381
29B6  E9 3C     SBC #3C
29B8  09 76     ORA #76
29BA  40        RTI 
29BB  FD 60 01  SBC 0160,X
29BE  9E          ???
29BF  00        BRK 
29C0  95 03     STA 03,X
29C2  24 28     BIT 28
29C4  8A        TXA 
29C5  76 5B     ROR 5B,X
29C7  42          ???
29C8  7F          ???
29C9  4D BC 65  EOR 65BC
29CC  33          ???
29CD  38        SEC 
29CE  4C 7F C9  JMP C97F
29D1  40        RTI 
29D2  AD 3B 6B  LDA 6B3B
29D5  86 53     STX 53
29D7  C8        INY 
29D8  DC          ???
29D9  9E          ???
29DA  91 57     STA (57),Y
29DC  A9 B4     LDA #B4
29DE  1A          ???
29DF  66 6F     ROR 6F
29E1  0D 59 74  ORA 7459
29E4  42          ???
29E5  D5 70     CMP 70,X
29E7  2C 19 62  BIT 6219
29EA  8A        TXA 
29EB  EB          ???
29EC  78        SEI 
29ED  E5 27     SBC 27
29EF  C9 92     CMP #92
29F1  11 9F     ORA (9F),Y
29F3  2C 00 2C  BIT 2C00
29F6  8D F5 02  STA 02F5
29F9  9F          ???
29FA  22          ???
29FB  1C          ???
29FC  67          ???
29FD  00        BRK 
29FE  58        CLI 
29FF  3F          ???
2A00  0A        ASL A
2A01  A7          ???
2A02  2C B4 11  BIT 11B4
2A05  C0 93     CPY #93
2A07  E8        INX 
2A08  8B          ???
2A09  4E 77 53  LSR 5377
2A0C  2C EB 69  BIT 69EB
2A0F  60        RTS 
2A10  9E          ???
2A11  B6 4E     LDX 4E,Y
2A13  1B          ???
2A14  3C          ???
2A15  DD 5E 9F  CMP 9F5E,X
2A18  2D C7 CF  AND CFC7
2A1B  9B          ???
2A1C  29 D8     AND #D8
2A1E  5D B9 33  EOR 33B9,X
2A21  0D 2A 81  ORA 812A
2A24  2D 8E 0E  AND 0E8E
2A27  3D 60 58  AND 5860,X
2A2A  E4 DA     CPX DA
2A2C  54          ???
2A2D  43          ???
2A2E  CD E2 04  CMP 04E2
2A31  78        SEI 
2A32  4A        LSR A
2A33  DD E4 9E  CMP 9EE4,X
2A36  87          ???
2A37  34          ???
2A38  11 AB     ORA (AB),Y
2A3A  12          ???
2A3B  A2 68     LDX #68
2A3D  6A        ROR A
2A3E  CB          ???
2A3F  07          ???
2A40  38        SEC 
2A41  E7          ???
2A42  DB          ???
2A43  2A        ROL A
2A44  D0 08     BNE 2A4E
2A46  13          ???
2A47  10 09     BPL 2A52
2A49  6E 7B 6A  ROR 6A7B
2A4C  44          ???
2A4D  64          ???
2A4E  05 CE     ORA CE
2A50  55 06     EOR 06,X
2A52  8F          ???
2A53  15 04     ORA 04,X
2A55  76 CA     ROR CA,X
2A57  7D 1A 8E  ADC 8E1A,X
2A5A  DE F1 2C  DEC 2CF1,X
2A5D  6B          ???
2A5E  09 95     ORA #95
2A60  62          ???
2A61  E2          ???
2A62  AD A3 30  LDA 30A3
2A65  B2          ???
2A66  BD B8 2C  LDA 2CB8,X
2A69  DB          ???
2A6A  58        CLI 
2A6B  65 AE     ADC AE
2A6D  0E F2 D3  ASL D3F2
2A70  2C 72 55  BIT 5572
2A73  34          ???
2A74  0F          ???
2A75  8B          ???
2A76  B4 80     LDY 80,X
2A78  6B          ???
2A79  17          ???
2A7A  D7          ???
2A7B  0A        ASL A
2A7C  4A        LSR A
2A7D  85 F1     STA F1
2A7F  EB          ???
2A80  AD 8B 26  LDA 268B
2A83  F5 41     SBC 41,X
2A85  59 23 3D  EOR 3D23,Y
2A88  60        RTS 
2A89  D8        CLD 
2A8A  CF          ???
2A8B  2C 65 40  BIT 4065
2A8E  39 65 44  AND 4465,Y
2A91  F5 05     SBC 05,X
2A93  D8        CLD 
2A94  13          ???
2A95  11 CE     ORA (CE),Y
2A97  47          ???
2A98  B2          ???
2A99  01 12     ORA (12,X)
2A9B  58        CLI 
2A9C  FB          ???
2A9D  4E DF 11  LSR 11DF
2AA0  5B          ???
2AA1  81 65     STA (65,X)
2AA3  EE FB 8D  INC 8DFB
2AA6  75 93     ADC 93,X
2AA8  49 99     EOR #99
2AAA  C1 17     CMP (17,X)
2AAC  32          ???
2AAD  0E B8 F5  ASL F5B8
2AB0  75 04     ADC 04,X
2AB2  64          ???
2AB3  8A        TXA 
2AB4  73          ???
2AB5  55 68     EOR 68,X
2AB7  4E DF 30  LSR 30DF
2ABA  C5 02     CMP 02
2ABC  A9 BB     LDA #BB
2ABE  E5 32     SBC 32
2AC0  01 0D     ORA (0D,X)
2AC2  99 4A 07  STA 074A,Y
2AC5  EB          ???
2AC6  D2          ???
2AC7  EE 43 71  INC 7143
2ACA  FB          ???
2ACB  46 66     LSR 66
2ACD  C0 52     CPY #52
2ACF  82          ???
2AD0  FB          ???
2AD1  56 7B     LSR 7B,X
2AD3  43          ???
2AD4  AF          ???
2AD5  71 CB     ADC (CB),Y
2AD7  E8        INX 
2AD8  E2          ???
2AD9  ED D0 74  SBC 74D0
2ADC  04          ???
2ADD  AC 92 5E  LDY 5E92
2AE0  AB          ???
2AE1  94 AA     STY AA,X
2AE3  DB          ???
2AE4  E9 54     SBC #54
2AE6  6B          ???
2AE7  97          ???
2AE8  DB          ???
2AE9  34          ???
2AEA  9C          ???
2AEB  90 86     BCC 2A73
2AED  B9 4B 48  LDA 484B,Y
2AF0  8D CA 2D  STA 2DCA
2AF3  9D F3 42  STA 42F3,X
2AF6  D7          ???
2AF7  4D 7B 52  EOR 527B
2AFA  A0 D7     LDY #D7
2AFC  D5 92     CMP 92,X
2AFE  AA        TAX 
2AFF  50 55     BVC 2B56
2B01  06 BB     ASL BB
2B03  99 15 52  STA 5215,Y
2B06  EC 47 9E  CPX 9E47
2B09  CD 02 53  CMP 5302
2B0C  4B          ???
2B0D  82          ???
2B0E  BD B6 8C  LDA 8CB6,X
2B11  43          ???
2B12  EE 4C 69  INC 694C
2B15  61 52     ADC (52,X)
2B17  B5 2E     LDA 2E,X
2B19  F9 E4 CC  SBC CCE4,Y
2B1C  94 58     STY 58,X
2B1E  30 A2     BMI 2AC2
2B20  4D DD 40  EOR 40DD
2B23  B1 7C     LDA (7C),Y
2B25  65 B0     ADC B0
2B27  0F          ???
2B28  FA          ???
2B29  4A        LSR A
2B2A  1B          ???
2B2B  5E C8 78  LSR 78C8,X
2B2E  74          ???
2B2F  00        BRK 
2B30  0E 0C 46  ASL 460C
2B33  9E          ???
2B34  0D 39 99  ORA 9939
2B37  E3          ???
2B38  C2          ???
2B39  AA        TAX 
2B3A  C0 17     CPY #17
2B3C  F6 8C     INC 8C,X
2B3E  AA        TAX 
2B3F  80          ???
2B40  0C          ???
2B41  3A          ???
2B42  88        DEY 
2B43  4B          ???
2B44  71 97     ADC (97),Y
2B46  13          ???
2B47  3A          ???
2B48  3C          ???
2B49  14          ???
2B4A  54          ???
2B4B  24 A8     BIT A8
2B4D  F2          ???
2B4E  08        PHP 
2B4F  B9 43 E7  LDA E743,Y
2B52  91 42     STA (42),Y
2B54  17          ???
2B55  08        PHP 
2B56  CD F4 A0  CMP A0F4
2B59  5C          ???
2B5A  EA        NOP 
2B5B  33          ???
2B5C  F5 11     SBC 11,X
2B5E  B9 B5 AD  LDA ADB5,Y
2B61  C2          ???
2B62  A7          ???
2B63  02          ???
2B64  A4 E2     LDY E2
2B66  94 04     STY 04,X
2B68  92          ???
2B69  83          ???
2B6A  8C B2 14  STY 14B2
2B6D  7A          ???
2B6E  D7          ???
2B6F  99 8D 74  STA 748D,Y
2B72  0D 38 1E  ORA 1E38
2B75  DB          ???
2B76  81 34     STA (34,X)
2B78  76 6E     ROR 6E,X
2B7A  97          ???
2B7B  96 D8     STX D8,Y
2B7D  B6 2D     LDX 2D,Y
2B7F  6A        ROR A
2B80  DC          ???
2B81  68        PLA 
2B82  D2          ???
2B83  45 15     EOR 15
2B85  B8        CLV 
2B86  38        SEC 
2B87  F4          ???
2B88  2C 6C 08  BIT 086C
2B8B  70 EA     BVS 2B77
2B8D  48        PHA 
2B8E  EE F0 E3  INC E3F0
2B91  5E 09 B2  LSR B209,X
2B94  A2 54     LDX #54
2B96  57          ???
2B97  3F          ???
2B98  35 DC     AND DC,X
2B9A  50 49     BVC 2BE5
2B9C  55 30     EOR 30,X
2B9E  D3          ???
2B9F  5A          ???
2BA0  B6 4B     LDX 4B,Y
2BA2  EB          ???
2BA3  06 DF     ASL DF
2BA5  8A        TXA 
2BA6  F6 51     INC 51,X
2BA8  92          ???
2BA9  F3          ???
2BAA  2D C5 66  AND 66C5
2BAD  74          ???
2BAE  57          ???
2BAF  B4 68     LDY 68,X
2BB1  B3          ???
2BB2  D8        CLD 
2BB3  49 5D     EOR #5D
2BB5  58        CLI 
2BB6  9C          ???
2BB7  22          ???
2BB8  48        PHA 
2BB9  8D 30 58  STA 5830
2BBC  56 94     LSR 94,X
2BBE  8D A4 CE  STA CEA4
2BC1  40        RTI 
2BC2  14          ???
2BC3  42          ???
2BC4  6B          ???
2BC5  06 19     ASL 19
2BC7  BD 60 A0  LDA A060,X
2BCA  61 08     ADC (08,X)
2BCC  15 AF     ORA AF,X
2BCE  CA        DEX 
2BCF  06 4B     ASL 4B
2BD1  D3          ???
2BD2  B0 D0     BCS 2BA4
2BD4  F3          ???
2BD5  11 8B     ORA (8B),Y
2BD7  C8        INY 
2BD8  95 ED     STA ED,X
2BDA  45 0E     EOR 0E
2BDC  F9 96 6B  SBC 6B96,Y
2BDF  3B          ???
2BE0  BA        TSX 
2BE1  24 20     BIT 20
2BE3  CA        DEX 
2BE4  59 0B 4D  EOR 4D0B,Y
2BE7  D3          ???
2BE8  77          ???
2BE9  FB          ???
2BEA  3B          ???
2BEB  41 D4     EOR (D4,X)
2BED  AC 01 B8  LDY B801
2BF0  A5 20     LDA 20
2BF2  DD E6 ED  CMP EDE6,X
2BF5  20 6E F7  JSR F76E
2BF8  07          ???
2BF9  0C          ???
2BFA  53          ???
2BFB  98        TYA 
2BFC  79 3B CD  ADC CD3B,Y
2BFF  F8        SED 
2C00  32          ???
2C01  24 91     BIT 91
2C03  3E D6 67  ROL 67D6,X
2C06  A8        TAY 
2C07  2B          ???
2C08  9E          ???
2C09  38        SEC 
2C0A  D7          ???
2C0B  21 90     AND (90,X)
2C0D  26 DA     ROL DA
2C0F  8D 24 19  STA 1924
2C12  B1 46     LDA (46),Y
2C14  1A          ???
2C15  19 2E 93  ORA 932E,Y
2C18  80          ???
2C19  44          ???
2C1A  0C          ???
2C1B  7B          ???
2C1C  FE 1E 6D  INC 6D1E,X
2C1F  36 8D     ROL 8D,X
2C21  6B          ???
2C22  9D E1 9A  STA 9AE1,X
2C25  C0 A5     CPY #A5
2C27  DC          ???
2C28  0F          ???
2C29  BA        TSX 
2C2A  8A        TXA 
2C2B  8F          ???
2C2C  70 60     BVS 2C8E
2C2E  0B          ???
2C2F  E2          ???
2C30  61 E4     ADC (E4,X)
2C32  C2          ???
2C33  2E 0E 66  ROL 660E
2C36  E4 E4     CPX E4
2C38  D3          ???
2C39  20 47 9B  JSR 9B47
2C3C  38        SEC 
2C3D  29 47     AND #47
2C3F  95 0A     STA 0A,X
2C41  B7          ???
2C42  A6 8E     LDX 8E
2C44  84 08     STY 08
2C46  EC 9A 01  CPX 019A
2C49  93          ???
2C4A  13          ???
2C4B  26 41     ROL 41
2C4D  4D 2C 5D  EOR 5D2C
2C50  C3          ???
2C51  78        SEI 
2C52  A7          ???
2C53  6C F7 6B  JMP (6BF7)
2C56  A4 E7     LDY E7
2C58  43          ???
2C59  16 7D     ASL 7D,X
2C5B  F6 94     INC 94,X
2C5D  FD 76 D5  SBC D576,X
2C60  14          ???
2C61  8D 9F B0  STA B09F
2C64  A0 3B     LDY #3B
2C66  56 AA     LSR AA,X
2C68  13          ???
2C69  72          ???
2C6A  30 1E     BMI 2C8A
2C6C  01 4F     ORA (4F,X)
2C6E  6E 30 29  ROR 2930
2C71  2C EF F1  BIT F1EF
2C74  E8        INX 
2C75  01 5A     ORA (5A,X)
2C77  FA          ???
2C78  E2          ???
2C79  A7          ???
2C7A  70 CF     BVS 2C4B
2C7C  A8        TAY 
2C7D  22          ???
2C7E  53          ???
2C7F  CF          ???
2C80  44          ???
2C81  C0 24     CPY #24
2C83  22          ???
2C84  15 02     ORA 02,X
2C86  C4 44     CPY 44
2C88  D2          ???
2C89  93          ???
2C8A  12          ???
2C8B  3D 15 B2  AND B215,X
2C8E  59 8D 0B  EOR 0B8D,Y
2C91  48        PHA 
2C92  13          ???
2C93  A7          ???
2C94  F1 D9     SBC (D9),Y
2C96  34          ???
2C97  4D 70 33  EOR 3370
2C9A  4C ED 7F  JMP 7FED
2C9D  A0 A8     LDY #A8
2C9F  A4 1C     LDY 1C
2CA1  03          ???
2CA2  CB          ???
2CA3  DF          ???
2CA4  0D BD 91  ORA 91BD
2CA7  46 2B     LSR 2B
2CA9  25 5B     AND 5B
2CAB  45 9C     EOR 9C
2CAD  1D 66 BA  ORA BA66,X
2CB0  2E C3 43  ROL 43C3
2CB3  EE E1 8C  INC 8CE1
2CB6  CB          ???
2CB7  1A          ???
2CB8  94 8D     STY 8D,X
2CBA  6B          ???
2CBB  FF          ???
2CBC  44          ???
2CBD  92          ???
2CBE  74          ???
2CBF  16 0B     ASL 0B,X
2CC1  C9 C6     CMP #C6
2CC3  28        PLP 
2CC4  32          ???
2CC5  35 E3     AND E3,X
2CC7  A1 82     LDA (82,X)
2CC9  6F          ???
2CCA  53          ???
2CCB  86 EC     STX EC
2CCD  F7          ???
2CCE  29 36     AND #36
2CD0  FE D4 1E  INC 1ED4,X
2CD3  4E 06 BE  LSR BE06
2CD6  17          ???
2CD7  B9 4D 77  LDA 774D,Y
2CDA  91 83     STA (83),Y
2CDC  4D 1D 78  EOR 781D
2CDF  07          ???
2CE0  58        CLI 
2CE1  5D 6A 58  EOR 586A,X
2CE4  1D C7 68  ORA 68C7,X
2CE7  F4          ???
2CE8  F7          ???
2CE9  66 14     ROR 14
2CEB  13          ???
2CEC  A7          ???
2CED  07          ???
2CEE  D1 10     CMP (10),Y
2CF0  A7          ???
2CF1  CC C8 B9  CPY B9C8
2CF4  68        PLA 
2CF5  51 40     EOR (40),Y
2CF7  A1 03     LDA (03,X)
2CF9  2D D1 AF  AND AFD1
2CFC  41 D6     EOR (D6,X)
2CFE  63          ???
2CFF  62          ???
2D00  E0 EB     CPX #EB
2D02  69 7E     ADC #7E
2D04  D1 2E     CMP (2E),Y
2D06  88        DEY 
2D07  E6 F8     INC F8
2D09  BD C0 14  LDA 14C0,X
2D0C  65 B6     ADC B6
2D0E  19 34 D9  ORA D934,Y
2D11  49 92     EOR #92
2D13  8D 4D 9B  STA 9B4D
2D16  26 B4     ROL B4
2D18  D6 7D     DEC 7D,X
2D1A  37          ???
2D1B  09 32     ORA #32
2D1D  9A        TXS 
2D1E  D4          ???
2D1F  E4 6E     CPX 6E
2D21  0B          ???
2D22  77          ???
2D23  92          ???
2D24  06 D0     ASL D0
2D26  DB          ???
2D27  AE 4D 70  LDX 704D
2D2A  09 C4     ORA #C4
2D2C  46 A3     LSR A3
2D2E  69 C8     ADC #C8
2D30  59 59 BB  EOR BB59,Y
2D33  1C          ???
2D34  29 9D     AND #9D
2D36  1B          ???
2D37  8D 3B B8  STA B83B
2D3A  60        RTS 
2D3B  31 48     AND (48),Y
2D3D  CC 6C 26  CPY 266C
2D40  9B          ???
2D41  03          ???
2D42  00        BRK 
2D43  2C EA E9  BIT E9EA
2D46  0E D8 E8  ASL E8D8
2D49  58        CLI 
2D4A  A9 AA     LDA #AA
2D4C  20 1E C7  JSR C71E
2D4F  60        RTS 
2D50  48        PHA 
2D51  DC          ???
2D52  F1 05     SBC (05),Y
2D54  2D B7 4F  AND 4FB7
2D57  AC 93 11  LDY 1193
2D5A  9D 1C 8F  STA 8F1C,X
2D5D  4D EF 99  EOR 99EF
2D60  DE 6E 42  DEC 426E,X
2D63  0B          ???
2D64  D0 23     BNE 2D89
2D66  B0 F9     BCS 2D61
2D68  2B          ???
2D69  D8        CLD 
2D6A  8A        TXA 
2D6B  2F          ???
2D6C  B2          ???
2D6D  06 52     ASL 52
2D6F  C1 57     CMP (57,X)
2D71  49 1D     EOR #1D
2D73  F8        SED 
2D74  4D 1E E8  EOR E81E
2D77  20 B2 50  JSR 50B2
2D7A  66 BE     ROR BE
2D7C  AB          ???
2D7D  6C 85 A5  JMP (A585)
2D80  D8        CLD 
2D81  8E 8C A3  STX A38C
2D84  71 2B     ADC (2B),Y
2D86  98        TYA 
2D87  B0 4E     BCS 2DD7
2D89  E8        INX 
2D8A  10 CA     BPL 2D56
2D8C  49 C7     EOR #C7
2D8E  5D 62 78  EOR 7862,X
2D91  D8        CLD 
2D92  C5 37     CMP 37
2D94  42          ???
2D95  47          ???
2D96  4D E9 06  EOR 06E9
2D99  0D B3 79  ORA 79B3
2D9C  2D B8 3E  AND 3EB8
2D9F  CA        DEX 
2DA0  8B          ???
2DA1  8D 44 E2  STA E244
2DA4  54          ???
2DA5  1C          ???
2DA6  94 25     STY 25,X
2DA8  41 63     EOR (63,X)
2DAA  35 2E     AND 2E,X
2DAC  30 B0     BMI 2D5E
2DAE  8E 7C 65  STX 657C
2DB1  A8        TAY 
2DB2  1B          ???
2DB3  2C 5A 46  BIT 465A
2DB6  48        PHA 
2DB7  2E 58 D3  ROL D358
2DBA  80          ???
2DBB  D5 60     CMP 60,X
2DBD  98        TYA 
2DBE  03          ???
2DBF  68        PLA 
2DC0  42          ???
2DC1  7C          ???
2DC2  8F          ???
2DC3  59 CA EF  EOR EFCA,Y
2DC6  38        SEC 
2DC7  AF          ???
2DC8  31 73     AND (73),Y
2DCA  AB          ???
2DCB  54          ???
2DCC  B9 51 C5  LDA C551,Y
2DCF  92          ???
2DD0  83          ???
2DD1  5C          ???
2DD2  06 36     ASL 36
2DD4  F6 45     INC 45,X
2DD6  EF          ???
2DD7  91 32     STA (32),Y
2DD9  8D 2E A1  STA A12E
2DDC  93          ???
2DDD  68        PLA 
2DDE  4C 01 4E  JMP 4E01
2DE1  DC          ???
2DE2  99 76 4F  STA 4F76,Y
2DE5  0C          ???
2DE6  BB          ???
2DE7  85 5C     STA 5C
2DE9  16 C5     ASL C5,X
2DEB  E9 AD     SBC #AD
2DED  2A        ROL A
2DEE  06 F1     ASL F1
2DF0  5B          ???
2DF1  2E 9F 42  ROL 429F
2DF4  D4          ???
2DF5  46 01     LSR 01
2DF7  03          ???
2DF8  4E C6 35  LSR 35C6
2DFB  BB          ???
2DFC  A9 F2     LDA #F2
2DFE  46 39     LSR 39
2E00  06 DE     ASL DE
2E02  75 52     ADC 52,X
2E04  3A          ???
2E05  5F          ???
2E06  5A          ???
2E07  07          ???
2E08  8D B5 24  STA 24B5
2E0B  40        RTI 
2E0C  CA        DEX 
2E0D  97          ???
2E0E  91 0F     STA (0F),Y
2E10  6E DD 40  ROR 40DD
2E13  75 62     ADC 62,X
2E15  F5 18     SBC 18,X
2E17  45 A9     EOR A9
2E19  99 09 82  STA 8209,Y
2E1C  06 5C     ASL 5C
2E1E  0D 9E 69  ORA 699E
2E21  BA        TSX 
2E22  15 F6     ORA F6,X
2E24  C0 05     CPY #05
2E26  93          ???
2E27  15 4D     ORA 4D,X
2E29  7B          ???
2E2A  8A        TXA 
2E2B  98        TYA 
2E2C  B5 95     LDA 95,X
2E2E  45 E5     EOR E5
2E30  0D BE 89  ORA 89BE
2E33  F0 F0     BEQ 2E25
2E35  B5 60     LDA 60,X
2E37  88        DEY 
2E38  35 13     AND 13,X
2E3A  A4 71     LDY 71
2E3C  0B          ???
2E3D  7A          ???
2E3E  66 63     ROR 63
2E40  04          ???
2E41  37          ???
2E42  A0 E3     LDY #E3
2E44  62          ???
2E45  EF          ???
2E46  18        CLC 
2E47  C8        INY 
2E48  25 0D     AND 0D
2E4A  CE 03 6E  DEC 6E03
2E4D  A7          ???
2E4E  B0 FD     BCS 2E4D
2E50  D4          ???
2E51  D5 9A     CMP 9A,X
2E53  08        PHP 
2E54  81 97     STA (97,X)
2E56  62          ???
2E57  58        CLI 
2E58  8D D3 54  STA 54D3
2E5B  A3          ???
2E5C  55 7E     EOR 7E,X
2E5E  09 D7     ORA #D7
2E60  46 99     LSR 99
2E62  05 62     ORA 62
2E64  5F          ???
2E65  B6 01     LDX 01,Y
2E67  18        CLC 
2E68  8F          ???
2E69  4D D2 C2  EOR C2D2
2E6C  B1 1A     LDA (1A),Y
2E6E  AB          ???
2E6F  07          ???
2E70  C6 0D     DEC 0D
2E72  8D FB 9D  STA 9DFB
2E75  8E 4F 25  STX 254F
2E78  3D 20 EC  AND EC20,X
2E7B  E2          ???
2E7C  CF          ???
2E7D  32          ???
2E7E  E8        INX 
2E7F  5A          ???
2E80  7E C9 27  ROR 27C9,X
2E83  AF          ???
2E84  08        PHP 
2E85  74          ???
2E86  8B          ???
2E87  AB          ???
2E88  07          ???
2E89  7B          ???
2E8A  06 71     ASL 71
2E8C  50 86     BVC 2E14
2E8E  7D 97 25  ADC 2597,X
2E91  4B          ???
2E92  41 2B     EOR (2B,X)
2E94  EF          ???
2E95  8D 04 7F  STA 7F04
2E98  29 44     AND #44
2E9A  2D EE 5D  AND 5DEE
2E9D  F1 EE     SBC (EE),Y
2E9F  19 71 B5  ORA B571,Y
2EA2  C4 44     CPY 44
2EA4  B2          ???
2EA5  D3          ???
2EA6  EC 4B 73  CPX 734B
2EA9  AB          ???
2EAA  CC 01 8E  CPY 8E01
2EAD  EB          ???
2EAE  72          ???
2EAF  14          ???
2EB0  77          ???
2EB1  51 CC     EOR (CC),Y
2EB3  0A        ASL A
2EB4  66 02     ROR 02
2EB6  17          ???
2EB7  02          ???
2EB8  A8        TAY 
2EB9  18        CLC 
2EBA  2D B7 14  AND 14B7
2EBD  57          ???
2EBE  6F          ???
2EBF  24 AD     BIT AD
2EC1  EA        NOP 
2EC2  0D 35 94  ORA 9435
2EC5  44          ???
2EC6  5C          ???
2EC7  95 04     STA 04,X
2EC9  55 0A     EOR 0A,X
2ECB  A6 5C     LDX 5C
2ECD  30 B5     BMI 2E84
2ECF  0E 27 CE  ASL CE27
2ED2  C1 90     CMP (90,X)
2ED4  8D 04 7E  STA 7E04
2ED7  B5 86     LDA 86,X
2ED9  EB          ???
2EDA  0D 02 A9  ORA A902
2EDD  9B          ???
2EDE  11 5B     ORA (5B),Y
2EE0  A3          ???
2EE1  20 22 0B  JSR 0B22
2EE4  BB          ???
2EE5  C0 81     CPY #81
2EE7  A1 22     LDA (22,X)
2EE9  CD 8D A2  CMP A28D
2EEC  58        CLI 
2EED  26 08     ROL 08
2EEF  02          ???
2EF0  C9 69     CMP #69
2EF2  AE 54 A6  LDX A654
2EF5  2C B7 86  BIT 86B7
2EF8  C2          ???
2EF9  C8        INY 
2EFA  94 C5     STY C5,X
2EFC  15 94     ORA 94,X
2EFE  42          ???
2EFF  54          ???
2F00  6B          ???
2F01  69 90     ADC #90
2F03  D0 C8     BNE 2ECD
2F05  82          ???
2F06  E1 93     SBC (93,X)
2F08  25 B1     AND B1
2F0A  8D 81 51  STA 5181
2F0D  F2          ???
2F0E  32          ???
2F0F  2E 79 44  ROL 4479
2F12  51 6A     EOR (6A),Y
2F14  17          ???
2F15  86 CE     STX CE
2F17  28        PLP 
2F18  A8        TAY 
2F19  29 1D     AND #1D
2F1B  AE A4 14  LDX 14A4
2F1E  68        PLA 
2F1F  C8        INY 
2F20  80          ???
2F21  1A          ???
2F22  64          ???
2F23  54          ???
2F24  B2          ???
2F25  A5 E5     LDA E5
2F27  A2 EB     LDX #EB
2F29  BB          ???
2F2A  8D 25 24  STA 2425
2F2D  D8        CLD 
2F2E  30 CD     BMI 2EFD
2F30  13          ???
2F31  DD 81 95  CMP 9581,X
2F34  C5 84     CMP 84
2F36  13          ???
2F37  42          ???
2F38  A1 8D     LDA (8D,X)
2F3A  41 BB     EOR (BB,X)
2F3C  12          ???
2F3D  B2          ???
2F3E  19 82 4D  ORA 4D82,Y
2F41  0B          ???
2F42  83          ???
2F43  4B          ???
2F44  CD 03 46  CMP 4603
2F47  07          ???
2F48  35 7F     AND 7F,X
2F4A  A9 86     LDA #86
2F4C  1D 4B D4  ORA D44B,X
2F4F  38        SEC 
2F50  F6 40     INC 40,X
2F52  5C          ???
2F53  A1 AF     LDA (AF,X)
2F55  77          ???
2F56  1A          ???
2F57  5B          ???
2F58  E1 6C     SBC (6C,X)
2F5A  48        PHA 
2F5B  48        PHA 
2F5C  8D 02 01  STA 0102
2F5F  1E 4C B8  ASL B84C,X
2F62  06 A2     ASL A2
2F64  8F          ???
2F65  EB          ???
2F66  E7          ???
2F67  DC          ???
2F68  F3          ???
2F69  22          ???
2F6A  44          ???
2F6B  B0 7C     BCS 2FE9
2F6D  92          ???
2F6E  3C          ???
2F6F  9A        TXS 
2F70  80          ???
2F71  06 B1     ASL B1
2F73  EC 31 36  CPX 3631
2F76  35 EF     AND EF,X
2F78  50 66     BVC 2FE0
2F7A  FB          ???
2F7B  4D 08 BC  EOR BC08
2F7E  96 F1     STX F1,Y
2F80  BB          ???
2F81  56 ED     LSR ED,X
2F83  3C          ???
2F84  AB          ???
2F85  45 7A     EOR 7A
2F87  E4 A1     CPX A1
2F89  74          ???
2F8A  8E 54 CC  STX CC54
2F8D  6C 0D 6A  JMP (6A0D)
2F90  CC 97 A6  CPY A697
2F93  4D 63 F8  EOR F863
2F96  8E 86 46  STX 4686
2F99  61 9D     ADC (9D,X)
2F9B  66 4F     ROR 4F
2F9D  33          ???
2F9E  7A          ???
2F9F  6D B2 96  ADC 96B2
2FA2  57          ???
2FA3  80          ???
2FA4  C4 82     CPY 82
2FA6  44          ???
2FA7  C6 BE     DEC BE
2FA9  CB          ???
2FAA  12          ???
2FAB  BA        TSX 
2FAC  A8        TAY 
2FAD  D5 A1     CMP A1,X
2FAF  BD 19 0D  LDA 0D19,X
2FB2  DA          ???
2FB3  33          ???
2FB4  55 04     EOR 04,X
2FB6  B9 87 08  LDA 0887,Y
2FB9  6A        ROR A
2FBA  D1 50     CMP (50),Y
2FBC  CA        DEX 
2FBD  92          ???
2FBE  54          ???
2FBF  5B          ???
2FC0  4D 2E 60  EOR 602E
2FC3  B4 FB     LDY FB,X
2FC5  83          ???
2FC6  5E 88 F3  LSR F388,X
2FC9  13          ???
2FCA  08        PHP 
2FCB  A4 48     LDY 48
2FCD  13          ???
2FCE  D4          ???
2FCF  4F          ???
2FD0  19 97 B3  ORA B397,Y
2FD3  BB          ???
2FD4  54          ???
2FD5  07          ???
2FD6  CD E4 1B  CMP 1BE4
2FD9  7B          ???
2FDA  D8        CLD 
2FDB  46 18     LSR 18
2FDD  97          ???
2FDE  07          ???
2FDF  19 D6 66  ORA 66D6,Y
2FE2  0D 3D E9  ORA E93D
2FE5  2C 0A 0F  BIT 0F0A
2FE8  55 53     EOR 53,X
2FEA  66 47     ROR 47
2FEC  5A          ???
2FED  A8        TAY 
2FEE  76 72     ROR 72,X
2FF0  7C          ???
2FF1  D1 F4     CMP (F4),Y
2FF3  48        PHA 
2FF4  86 B0     STX B0
2FF6  53          ???
2FF7  D9 C6 8D  CMP 8DC6,Y
2FFA  0F          ???
2FFB  A1 90     LDA (90,X)
2FFD  51 A3     EOR (A3),Y
2FFF  B6 B0     LDX B0,Y
3001  75 C4     ADC C4,X
3003  43          ???
3004  47          ???
3005  3F          ???
3006  1A          ???
3007  83          ???
3008  2B          ???
3009  6E 27 37  ROR 3727
300C  8B          ???
300D  53          ???
300E  EB          ???
300F  A2 96     LDX #96
3011  63          ???
3012  27          ???
3013  D3          ???
3014  00        BRK 
3015  AD 19 27  LDA 2719
3018  D6 AA     DEC AA,X
301A  8F          ???
301B  99 A9 CB  STA CBA9,Y
301E  4C AA A6  JMP A6AA
3021  89          ???
3022  20 27 30  JSR 3027
3025  46 D3     LSR D3
3027  9A        TXS 
3028  CD 40 86  CMP 8640
302B  20 09 20  JSR 2009
302E  22          ???
302F  33          ???
3030  7E 02 C4  ROR C402,X
3033  B9 CA 30  LDA 30CA,Y
3036  8E B5 AE  STX AEB5
3039  AB          ???
303A  DD 0C 5A  CMP 5A0C,X
303D  8A        TXA 
303E  8B          ???
303F  58        CLI 
3040  D3          ???
3041  98        TYA 
3042  16 48     ASL 48,X
3044  8F          ???
3045  0E 1D 7D  ASL 7D1D
3048  0F          ???
3049  18        CLC 
304A  70 50     BVS 309C
304C  D5 75     CMP 75,X
304E  03          ???
304F  1B          ???
3050  48        PHA 
3051  60        RTS 
3052  65 A6     ADC A6
3054  E2          ???
3055  2D 13 6F  AND 6F13
3058  46 9B     LSR 9B
305A  DA          ???
305B  34          ???
305C  C5 3B     CMP 3B
305E  1A          ???
305F  3F          ???
3060  94 4C     STY 4C,X
3062  F6 5A     INC 5A,X
3064  C5 9A     CMP 9A
3066  45 36     EOR 36
3068  62          ???
3069  7B          ???
306A  8D 52 3B  STA 3B52
306D  20 B6 0D  JSR 0DB6
3070  5B          ???
3071  74          ???
3072  4F          ???
3073  FA          ???
3074  47          ???
3075  E7          ???
3076  50 22     BVC 309A
3078  B3          ???
3079  35 22     AND 22,X
307B  2D 01 C2  AND C201
307E  95 AA     STA AA,X
3080  6D E5 AB  ADC ABE5
3083  AD 6B 53  LDA 536B
3086  9E          ???
3087  BD 41 62  LDA 6241,X
308A  C0 14     CPY #14
308C  17          ???
308D  2E 6F 8C  ROL 8C6F
3090  A0 B5     LDY #B5
3092  23          ???
3093  C5 67     CMP 67
3095  D1 BE     CMP (BE),Y
3097  E3          ???
3098  70 45     BVS 30DF
309A  B6 B5     LDX B5,Y
309C  8D F8 C7  STA C7F8
309F  88        DEY 
30A0  71 8A     ADC (8A),Y
30A2  29 23     AND #23
30A4  96 8C     STX 8C,Y
30A6  26 15     ROL 15
30A8  E0 CE     CPX #CE
30AA  DD 04 9D  CMP 9D04,X
30AD  05 03     ORA 03
30AF  CE B5 9E  DEC 9EB5
30B2  94 19     STY 19,X
30B4  F5 0B     SBC 0B,X
30B6  D7          ???
30B7  01 E3     ORA (E3,X)
30B9  43          ???
30BA  2A        ROL A
30BB  85 C3     STA C3
30BD  00        BRK 
30BE  B5 E1     LDA E1,X
30C0  24 D0     BIT D0
30C2  B8        CLV 
30C3  1C          ???
30C4  E7          ???
30C5  49 AE     EOR #AE
30C7  45 D5     EOR D5
30C9  C5 D4     CMP D4
30CB  2A        ROL A
30CC  6E 20 50  ROR 5020
30CF  D4          ???
30D0  1C          ???
30D1  EA        NOP 
30D2  92          ???
30D3  6A        ROR A
30D4  5C          ???
30D5  56 8D     LSR 8D,X
30D7  68        PLA 
30D8  28        PLP 
30D9  19 AD 50  ORA 50AD,Y
30DC  CA        DEX 
30DD  15 1C     ORA 1C,X
30DF  C7          ???
30E0  64          ???
30E1  26 A3     ROL A3
30E3  83          ???
30E4  DC          ???
30E5  C6 B7     DEC B7
30E7  EC 59 76  CPX 7659
30EA  8D 43 01  STA 0143
30ED  77          ???
30EE  55 F7     EOR F7,X
30F0  41 7A     EOR (7A,X)
30F2  F4          ???
30F3  FA          ???
30F4  55 26     EOR 26,X
30F6  A4 A8     LDY A8
30F8  2E 56 AA  ROL AA56
30FB  D5 1D     CMP 1D,X
30FD  01 A7     ORA (A7,X)
30FF  9E          ???
3100  A3          ???
3101  C1 74     CMP (74,X)
3103  B0 6F     BCS 3174
3105  D6 14     DEC 14,X
3107  63          ???
3108  38        SEC 
3109  C2          ???
310A  99 28 6B  STA 6B28,Y
310D  3B          ???
310E  23          ???
310F  DE 1F 17  DEC 171F,X
3112  BD 52 90  LDA 9052,X
3115  94 34     STY 34,X
3117  D3          ???
3118  D9 52 90  CMP 9052,Y
311B  0C          ???
311C  6B          ???
311D  0A        ASL A
311E  76 57     ROR 57,X
3120  AA        TAX 
3121  8E 19 AD  STX AD19
3124  F9 02 B3  SBC B302,Y
3127  CE 82 8D  DEC 8D82
312A  68        PLA 
312B  C5 0F     CMP 0F
312D  68        PLA 
312E  90 01     BCC 3131
3130  72          ???
3131  CA        DEX 
3132  5C          ???
3133  9B          ???
3134  8B          ???
3135  56 8A     LSR 8A,X
3137  4A        LSR A
3138  3A          ???
3139  ED 5B 54  SBC 545B
313C  CC A5 67  CPY 67A5
313F  10 7D     BPL 31BE
3141  13          ???
3142  8D 64 72  STA 7264
3145  A4 A7     LDY A7
3147  4A        LSR A
3148  3E 10 69  ROL 6910,X
314B  C1 B9     CMP (B9,X)
314D  E4 06     CPX 06
314F  4D 3E 75  EOR 753E
3152  1A          ???
3153  44          ???
3154  01 07     ORA (07,X)
3156  15 56     ORA 56,X
3158  C8        INY 
3159  14          ???
315A  40        RTI 
315B  2E C6 05  ROL 05C6
315E  1D 83 EF  ORA EF83,X
3161  D7          ???
3162  30 16     BMI 317A
3164  9A        TXS 
3165  03          ???
3166  83          ???
3167  CD 38 8D  CMP 8D38
316A  5E 19 01  LSR 0119,X
316D  8D 4E C4  STA C44E
3170  EA        NOP 
3171  C2          ???
3172  13          ???
3173  6B          ???
3174  3C          ???
3175  19 5A AA  ORA AA5A,Y
3178  33          ???
3179  52          ???
317A  62          ???
317B  64          ???
317C  0C          ???
317D  EE 35 8D  INC 8D35
3180  9A        TXS 
3181  06 16     ASL 16
3183  D6 05     DEC 05,X
3185  28        PLP 
3186  4A        LSR A
3187  43          ???
3188  C9 A8     CMP #A8
318A  35 CA     AND CA,X
318C  2A        ROL A
318D  F5 A4     SBC A4,X
318F  6A        ROR A
3190  16 EC     ASL EC,X
3192  73          ???
3193  55 CC     EOR CC,X
3195  9A        TXS 
3196  E6 37     INC 37
3198  F3          ???
3199  09 42     ORA #42
319B  5C          ???
319C  22          ???
319D  D7          ???
319E  50 F1     BVC 3191
31A0  39 37 2F  AND 2F37,Y
31A3  20 E1 60  JSR 60E1
31A6  2B          ???
31A7  16 B9     ASL B9,X
31A9  07          ???
31AA  B7          ???
31AB  82          ???
31AC  F6 81     INC 81,X
31AE  8D DF 57  STA 57DF
31B1  53          ???
31B2  34          ???
31B3  C9 17     CMP #17
31B5  CD 05 89  CMP 8905
31B8  04          ???
31B9  99 96 AC  STA AC96,Y
31BC  EC E1 52  CPX 52E1
31BF  A0 68     LDY #68
31C1  DC          ???
31C2  E4 8B     CPX 8B
31C4  93          ???
31C5  DA          ???
31C6  A5 AB     LDA AB
31C8  70 30     BVS 31FA
31CA  4B          ???
31CB  93          ???
31CC  3D 28 E6  AND E628,X
31CF  0C          ???
31D0  E9 DA     SBC #DA
31D2  D2          ???
31D3  DE 0E 6A  DEC 6A0E,X
31D6  5B          ???
31D7  43          ???
31D8  DE 43 CC  DEC CC43,X
31DB  F3          ???
31DC  AA        TAX 
31DD  58        CLI 
31DE  C6 2D     DEC 2D
31E0  95 AA     STA AA,X
31E2  39 5A F3  AND F35A,Y
31E5  8E 52 AD  STX AD52
31E8  90 B4     BCC 319E
31EA  4D 5A 11  EOR 115A
31ED  2F          ???
31EE  41 02     EOR (02,X)
31F0  6B          ???
31F1  B9 49 EA  LDA EA49,Y
31F4  EB          ???
31F5  01 B1     ORA (B1,X)
31F7  0B          ???
31F8  E2          ???
31F9  D7          ???
31FA  54          ???
31FB  09 0D     ORA #0D
31FD  0F          ???
31FE  14          ???
31FF  08        PHP 
3200  19 7F 28  ORA 287F,Y
3203  2C 24 B5  BIT B524
3206  4D AD D6  EOR D6AD
3209  53          ???
320A  BB          ???
320B  A6 40     LDX 40
320D  4D 03 B5  EOR B503
3210  0D 84 C6  ORA C684
3213  51 A3     EOR (A3),Y
3215  5A          ???
3216  86 C3     STX C3
3218  06 85     ASL 85
321A  90 6D     BCC 3289
321C  10 FC     BPL 321A
321E  58        CLI 
321F  41 97     EOR (97,X)
3221  2D 7D 13  AND 137D
3224  6E DD E7  ROR E7DD
3227  4A        LSR A
3228  4A        LSR A
3229  EA        NOP 
322A  42          ???
322B  12          ???
322C  6F          ???
322D  23          ???
322E  CD 8C E6  CMP E68C
3231  2E 41 F7  ROL F741
3234  35 13     AND 13,X
3236  4D 3A B4  EOR B43A
3239  EE 6B FA  INC FA6B
323C  14          ???
323D  24 14     BIT 14
323F  17          ???
3240  FC          ???
3241  DD 04 A8  CMP A804,X
3244  74          ???
3245  90 11     BCC 3258
3247  58        CLI 
3248  61 13     ADC (13,X)
324A  68        PLA 
324B  0F          ???
324C  BA        TSX 
324D  93          ???
324E  52          ???
324F  4C 2A 3D  JMP 3D2A
3252  60        RTS 
3253  5D D3 91  EOR 91D3,X
3256  A4 30     LDY 30
3258  85 32     STA 32
325A  35 55     AND 55,X
325C  73          ???
325D  BE BC 0D  LDX 0DBC,Y
3260  CC C4 00  CPY 00C4
3263  70 0B     BVS 3270
3265  15 AE     ORA AE,X
3267  CE 35 47  DEC 4735
326A  48        PHA 
326B  A6 C5     LDX C5
326D  6E EA 66  ROR 66EA
3270  13          ???
3271  31 54     AND (54),Y
3273  47          ???
3274  F1 D9     SBC (D9),Y
3276  B9 2C CB  LDA CB2C,Y
3279  71 A9     ADC (A9),Y
327B  06 00     ASL 00
327D  24 2E     BIT 2E
327F  BA        TSX 
3280  91 3A     STA (3A),Y
3282  36 22     ROL 22,X
3284  EA        NOP 
3285  54          ???
3286  FE 7A 22  INC 227A,X
3289  24 BF     BIT BF
328B  41 9D     EOR (9D,X)
328D  5F          ???
328E  6B          ???
328F  18        CLC 
3290  B9 54 C1  LDA C154,Y
3293  A6 DC     LDX DC
3295  EA        NOP 
3296  30 B5     BMI 324D
3298  16 88     ASL 88,X
329A  46 15     LSR 15
329C  78        SEI 
329D  BF          ???
329E  D5 6F     CMP 6F,X
32A0  99 D0 93  STA 93D0,Y
32A3  AC D8 AB  LDY ABD8
32A6  6C 98 9A  JMP (9A98)
32A9  7A          ???
32AA  AD 0A 83  LDA 830A
32AD  D2          ???
32AE  60        RTS 
32AF  D9 43 48  CMP 4843,Y
32B2  9E          ???
32B3  D5 4C     CMP 4C,X
32B5  34          ???
32B6  C8        INY 
32B7  28        PLP 
32B8  33          ???
32B9  6C 0B 0C  JMP (0C0B)
32BC  CD 15 BC  CMP BC15
32BF  29 8D     AND #8D
32C1  C2          ???
32C2  2C 4A FE  BIT FE4A
32C5  93          ???
32C6  72          ???
32C7  19 B9 57  ORA 57B9,Y
32CA  60        RTS 
32CB  D6 0C     DEC 0C,X
32CD  05 2D     ORA 2D
32CF  53          ???
32D0  48        PHA 
32D1  EE 99 82  INC 8299
32D4  AB          ???
32D5  3A          ???
32D6  62          ???
32D7  48        PHA 
32D8  13          ???
32D9  B7          ???
32DA  33          ???
32DB  E5 33     SBC 33
32DD  26 8B     ROL 8B
32DF  E6 6E     INC 6E
32E1  12          ???
32E2  20 09 25  JSR 2509
32E5  AD 50 46  LDA 4650
32E8  AA        TAX 
32E9  E6 41     INC 41
32EB  B0 27     BCS 3314
32ED  49 56     EOR #56
32EF  03          ???
32F0  16 2E     ASL 2E,X
32F2  A8        TAY 
32F3  D5 14     CMP 14,X
32F5  DC          ???
32F6  A7          ???
32F7  24 31     BIT 31
32F9  67          ???
32FA  E1 D7     SBC (D7,X)
32FC  94 D0     STY D0,X
32FE  1E 81 BD  ASL BD81,X
3301  41 43     EOR (43,X)
3303  1D 2E 80  ORA 802E,X
3306  96 00     STX 00,Y
3308  28        PLP 
3309  80          ???
330A  C6 A7     DEC A7
330C  91 00     STA (00),Y
330E  14          ???
330F  38        SEC 
3310  32          ???
3311  C3          ???
3312  92          ???
3313  19 A3 19  ORA 19A3,Y
3316  EA        NOP 
3317  FE 4A 5D  INC 5D4A,X
331A  50 85     BVC 32A1
331C  28        PLP 
331D  C5 12     CMP 12
331F  35 29     AND 29,X
3321  A2 B1     LDX #B1
3323  44          ???
3324  A8        TAY 
3325  43          ???
3326  BE 2A DA  LDX DA2A,Y
3329  41 A5     EOR (A5,X)
332B  2B          ???
332C  90 42     BCC 3370
332E  52          ???
332F  B3          ???
3330  85 C9     STA C9
3332  DC          ???
3333  B8        CLV 
3334  26 43     ROL 43
3336  20 C2 9A  JSR 9AC2
3339  09 A5     ORA #A5
333B  E7          ???
333C  44          ???
333D  37          ???
333E  26 38     ROL 38
3340  33          ???
3341  6F          ???
3342  4B          ???
3343  D5 2D     CMP 2D,X
3345  F6 2C     INC 2C,X
3347  5E 44 1C  LSR 1C44,X
334A  35 42     AND 42,X
334C  E8        INX 
334D  C6 B8     DEC B8
334F  5D 4C 44  EOR 444C,X
3352  6F          ???
3353  73          ???
3354  98        TYA 
3355  8E 5B 9D  STX 9D5B
3358  15 EB     ORA EB,X
335A  4D 2E 16  EOR 162E
335D  72          ???
335E  B0 2C     BCS 338C
3360  C9 3A     CMP #3A
3362  2A        ROL A
3363  97          ???
3364  0D 5D 12  ORA 125D
3367  74          ???
3368  48        PHA 
3369  15 04     ORA 04,X
336B  73          ???
336C  C3          ???
336D  58        CLI 
336E  46 4C     LSR 4C
3370  14          ???
3371  77          ???
3372  EE D3 11  INC 11D3
3375  37          ???
3376  39 DB B5  AND B5DB,Y
3379  4C 7C BD  JMP BD7C
337C  0B          ???
337D  56 7D     LSR 7D,X
337F  D3          ???
3380  74          ???
3381  DC          ???
3382  A3          ???
3383  8C DF A6  STY A6DF
3386  62          ???
3387  16 2B     ASL 2B,X
3389  55 10     EOR 10,X
338B  8E D0 4E  STX 4ED0
338E  44          ???
338F  88        DEY 
3390  9D 28 CC  STA CC28,X
3393  03          ???
3394  9C          ???
3395  8A        TXA 
3396  98        TYA 
3397  36 9C     ROL 9C,X
3399  CD 4F 74  CMP 744F
339C  29 9A     AND #9A
339E  66 43     ROR 43
33A0  4D 69 AA  EOR AA69
33A3  EC DD C6  CPX C6DD
33A6  48        PHA 
33A7  E8        INX 
33A8  6B          ???
33A9  AE 58 9D  LDX 9D58
33AC  35 EE     AND EE,X
33AE  98        TYA 
33AF  25 22     AND 22
33B1  54          ???
33B2  B9 D4 5C  LDA 5CD4,Y
33B5  36 9B     ROL 9B,X
33B7  4E AE A4  LSR A4AE
33BA  09 AA     ORA #AA
33BC  8C 88 2C  STY 2C88
33BF  1D 20 D5  ORA D520,X
33C2  33          ???
33C3  37          ???
33C4  38        SEC 
33C5  2D F8 31  AND 31F8
33C8  15 35     ORA 35,X
33CA  EA        NOP 
33CB  C6 49     DEC 49
33CD  A9 7A     LDA #7A
33CF  0E 1B 86  ASL 861B
33D2  43          ???
33D3  A4 96     LDY 96
33D5  60        RTS 
33D6  BF          ???
33D7  55 74     EOR 74,X
33D9  EB          ???
33DA  D6 DC     DEC DC,X
33DC  7A          ???
33DD  8C 2C A2  STY A22C
33E0  94 C1     STY C1,X
33E2  AA        TAX 
33E3  AA        TAX 
33E4  9A        TXS 
33E5  3A          ???
33E6  99 56 50  STA 5056,Y
33E9  FF          ???
33EA  7A          ???
33EB  CA        DEX 
33EC  D5 F5     CMP F5,X
33EE  33          ???
33EF  B2          ???
33F0  5B          ???
33F1  27          ???
33F2  EA        NOP 
33F3  B6 4D     LDX 4D,Y
33F5  27          ???
33F6  EB          ???
33F7  52          ???
33F8  57          ???
33F9  F6 41     INC 41,X
33FB  44          ???
33FC  56 BE     LSR BE,X
33FE  BB          ???
33FF  54          ???
3400  55 52     EOR 52,X
3402  5D B6 35  EOR 35B6,X
3405  46 2D     LSR 2D
3407  C0 DA     CPY #DA
3409  5A          ???
340A  42          ???
340B  EB          ???
340C  B5 49     LDA 49,X
340E  5A          ???
340F  4F          ???
3410  44          ???
3411  B8        CLV 
3412  23          ???
3413  06 10     ASL 10
3415  C2          ???
3416  6A        ROR A
3417  50 E4     BVC 33FD
3419  F0 64     BEQ 347F
341B  58        CLI 
341C  F2          ???
341D  54          ???
341E  7B          ???
341F  F8        SED 
3420  38        SEC 
3421  4D E0 DC  EOR DCE0
3424  D4          ???
3425  0B          ???
3426  DC          ???
3427  53          ???
3428  2D 09 B2  AND B209
342B  36 A4     ROL A4,X
342D  8E 14 BF  STX BF14
3430  A1 49     LDA (49,X)
3432  56 41     LSR 41,X
3434  EF          ???
3435  48        PHA 
3436  4F          ???
3437  4B          ???
3438  45 2E     EOR 2E
343A  F4          ???
343B  84 13     STY 13
343D  9A        TXS 
343E  15 CF     ORA CF,X
3440  46 C6     LSR C6
3442  04          ???
3443  B7          ???
3444  BA        TSX 
3445  4B          ???
3446  4B          ???
3447  76 7A     ROR 7A,X
3449  A5 AF     LDA AF
344B  9C          ???
344C  96 A5     STX A5,Y
344E  B9 5B 97  LDA 975B,Y
3451  6C 5A 6A  JMP (6A5A)
3454  47          ???
3455  B8        CLV 
3456  91 54     STA (54),Y
3458  87          ???
3459  57          ???
345A  74          ???
345B  C8        INY 
345C  81 1B     STA (1B,X)
345E  41 4D     EOR (4D,X)
3460  20 57 87  JSR 8757
3463  8A        TXA 
3464  52          ???
3465  4E 51 55  LSR 5551
3468  E2          ???
3469  5A          ???
346A  4F          ???
346B  52          ???
346C  EA        NOP 
346D  A9 18     LDA #18
346F  E2          ???
3470  61 0B     ADC (0B,X)
3472  6F          ???
3473  57          ???
3474  B4 DA     LDY DA,X
3476  C7          ???
3477  36 71     ROL 71,X
3479  8D 35 24  STA 2435
347C  CB          ???
347D  07          ???
347E  85 0E     STA 0E
3480  BC 59 EF  LDY EF59,X
3483  E5 5A     SBC 5A
3485  7A          ???
3486  2C 5C E6  BIT E65C
3489  19 4E 47  ORA 474E,Y
348C  4C 5D 92  JMP 925D
348F  48        PHA 
3490  2F          ???
3491  53          ???
3492  4F          ???
3493  43          ???
3494  2E 45 4B  ROL 4B45
3497  C9 C5     CMP #C5
3499  49 4E     EOR #4E
349B  6C 83 78  JMP (7883)
349E  C5 84     CMP 84
34A0  DA          ???
34A1  41 52     EOR (52,X)
34A3  D9 59 59  CMP 5959,Y
34A6  4D 49 60  EOR 6049
34A9  44          ???
34AA  4C BC 51  JMP 51BC
34AD  1F          ???
34AE  47          ???
34AF  5E AA A5  LSR A5AA,X
34B2  54          ???
34B3  81 36     STA (36,X)
34B5  2D 7A F6  AND F67A
34B8  37          ???
34B9  68        PLA 
34BA  8D 4E 05  STA 054E
34BD  6F          ???
34BE  B2          ???
34BF  59 F6 53  EOR 53F6,Y
34C2  0B          ???
34C3  3A          ???
34C4  5F          ???
34C5  50 2B     BVC 34F2
34C7  8D 07 42  STA 4207
34CA  D2          ???
34CB  C9 55     CMP #55
34CD  0C          ???
34CE  EE 58 8F  INC 8F58
34D1  35 31     AND 31,X
34D3  2E 8E 44  ROL 448E
34D6  7C          ???
34D7  C5 49     CMP 49
34D9  47          ???
34DA  74          ???
34DB  B9 52 27  LDA 2752,Y
34DE  74          ???
34DF  94 7E     STY 7E,X
34E1  4F          ???
34E2  54          ???
34E3  45 53     EOR 53
34E5  BF          ???
34E6  8C 10 40  STY 4010
34E9  17          ???
34EA  8D 4D E3  STA E34D
34ED  A9 7A     LDA #7A
34EF  90 46     BCC 3537
34F1  C3          ???
34F2  FA          ???
34F3  BC 8B 42  LDY 428B,X
34F6  5B          ???
34F7  A9 29     LDA #29
34F9  92          ???
34FA  EB          ???
34FB  8F          ???
34FC  BE AA 7E  LDX 7EAA,Y
34FF  02          ???
3500  AD 46 C0  LDA C046
3503  4F          ???
3504  C5 26     CMP 26
3506  A6 B2     LDX B2
3508  40        RTI 
3509  8A        TXA 
350A  28        PLP 
350B  2E A3 B7  ROL B7A3
350E  47          ???
350F  FB          ???
3510  43          ???
3511  22          ???
3512  22          ???
3513  57          ???
3514  BC C5 28  LDY 28C5,X
3517  50 6B     BVC 3584
3519  77          ???
351A  47          ???
351B  85 81     STA 81
351D  B3          ???
351E  22          ???
351F  AF          ???
3520  45 15     EOR 15
3522  A6 CC     LDX CC
3524  78        SEI 
3525  01 D6     ORA (D6,X)
3527  5B          ???
3528  71 2A     ADC (2A),Y
352A  20 B3 49  JSR 49B3
352D  22          ???
352E  56 97     LSR 97,X
3530  3A          ???
3531  29 12     AND #12
3533  58        CLI 
3534  12          ???
3535  E3          ???
3536  6A        ROR A
3537  2C 8B 45  BIT 458B
353A  65 6F     ADC 6F
353C  98        TYA 
353D  D8        CLD 
353E  17          ???
353F  48        PHA 
3540  65 9B     ADC 9B
3542  D6 9A     DEC 9A,X
3544  3A          ???
3545  9F          ???
3546  65 E4     ADC E4
3548  FF          ???
3549  36 7D     ROL 7D,X
354B  99 A5 3D  STA 3DA5,Y
354E  42          ???
354F  0B          ???
3550  A3          ???
3551  71 9D     ADC (9D),Y
3553  AA        TAX 
3554  F3          ???
3555  02          ???
3556  D6 94     DEC 94,X
3558  9B          ???
3559  85 01     STA 01
355B  19 0C 7B  ORA 7B0C,Y
355E  94 D3     STY D3,X
3560  2E 8D 4A  ROL 4A8D
3563  73          ???
3564  CC A3 68  CPY 68A3
3567  DB          ???
3568  0F          ???
3569  A6 B1     LDX B1
356B  08        PHP 
356C  2E F5 07  ROL 07F5
356F  1D 97 74  ORA 7497,X
3572  90 4B     BCC 35BF
3574  DB          ???
3575  58        CLI 
3576  8D 41 DF  STA DF41
3579  C9 15     CMP #15
357B  35 A5     AND A5,X
357D  6F          ???
357E  46 0C     LSR 0C
3580  4C 43 98  JMP 9843
3583  4A        LSR A
3584  90 B0     BCC 3536
3586  69 26     ADC #26
3588  38        SEC 
3589  74          ???
358A  4C 89 3F  JMP 3F89
358D  8D 22 B9  STA B922
3590  8C D4 81  STY 81D4
3593  55 10     EOR 10,X
3595  82          ???
3596  F3          ???
3597  29 3A     AND #3A
3599  1D 28 8D  ORA 8D28,X
359C  49 16     EOR #16
359E  F4          ???
359F  2B          ???
35A0  9A        TXS 
35A1  05 B1     ORA B1
35A3  47          ???
35A4  2C 61 0E  BIT 0E61
35A7  03          ???
35A8  A5 50     LDA 50
35AA  6D 2C 35  ADC 352C
35AD  C6 48     DEC 48
35AF  44          ???
35B0  BA        TSX 
35B1  04          ???
35B2  B3          ???
35B3  26 86     ROL 86
35B5  6A        ROR A
35B6  C1 B9     CMP (B9,X)
35B8  0E 10 94  ASL 9410
35BB  1D 4A 96  ORA 964A,X
35BE  E0 6F     CPX #6F
35C0  A8        TAY 
35C1  50 84     BVC 3547
35C3  DF          ???
35C4  2A        ROL A
35C5  85 02     STA 02
35C7  57          ???
35C8  46 CC     LSR CC
35CA  0D D6 A0  ORA A0D6
35CD  12          ???
35CE  48        PHA 
35CF  44          ???
35D0  5B          ???
35D1  58        CLI 
35D2  07          ???
35D3  92          ???
35D4  F8        SED 
35D5  94 54     STY 54,X
35D7  B4 10     LDY 10,X
35D9  58        CLI 
35DA  3E A0 69  ROL 69A0,X
35DD  21 E9     AND (E9,X)
35DF  01 E7     ORA (E7,X)
35E1  0B          ???
35E2  40        RTI 
35E3  F2          ???
35E4  D6 1E     DEC 1E,X
35E6  64          ???
35E7  DF          ???
35E8  A9 CE     LDA #CE
35EA  88        DEY 
35EB  49 1B     EOR #1B
35ED  0E 2F 2E  ASL 2E2F
35F0  01 08     ORA (08,X)
35F2  2D 8D 13  AND 138D
35F5  AF          ???
35F6  10 DD     BPL 35D5
35F8  7A          ???
35F9  0B          ???
35FA  A1 C4     LDA (C4,X)
35FC  54          ???
35FD  D4          ???
35FE  47          ???
35FF  A8        TAY 
3600  13          ???
3601  37          ???
3602  1D 34 20  ORA 2034,X
3605  D7          ???
3606  6D 61 1C  ADC 1C61
3609  C9 4A     CMP #4A
360B  8D BB 14  STA 14BB
360E  2C 7D A5  BIT A57D
3611  2D 08 D5  AND D508
3614  01 4D     ORA (4D,X)
3616  CC 9B 5A  CPY 5A9B
3619  37          ???
361A  30 E1     BMI 35FD
361C  86 7D     STX 7D
361E  33          ???
361F  45 04     EOR 04
3621  B5 8D     LDA 8D,X
3623  F7          ???
3624  92          ???
3625  59 08 41  EOR 4108,Y
3628  4C 12 65  JMP 6512
362B  4D 32 1C  EOR 1C32
362E  66 14     ROR 14
3630  72          ???
3631  33          ???
3632  26 2B     ROL 2B
3634  72          ???
3635  87          ???
3636  51 07     EOR (07),Y
3638  2B          ???
3639  65 9E     ADC 9E
363B  9C          ???
363C  1B          ???
363D  94 D9     STY D9,X
363F  31 11     AND (11),Y
3641  D6 9A     DEC 9A,X
3643  F2          ???
3644  F2          ???
3645  13          ???
3646  5B          ???
3647  86 06     STX 06
3649  20 4F 35  JSR 354F
364C  67          ???
364D  B2          ???
364E  9C          ???
364F  C8        INY 
3650  9D 2C 7B  STA 7B2C,X
3653  B6 A8     LDX A8,Y
3655  78        SEI 
3656  67          ???
3657  92          ???
3658  86 48     STX 48
365A  10 BC     BPL 3618
365C  C5 1A     CMP 1A
365E  B4 3F     LDY 3F,X
3660  8D 4A 9D  STA 9D4A
3663  5A          ???
3664  15 B8     ORA B8,X
3666  C4 4E     CPY 4E
3668  98        TYA 
3669  1A          ???
366A  52          ???
366B  02          ???
366C  63          ???
366D  94 E7     STY E7,X
366F  53          ???
3670  0F          ???
3671  1B          ???
3672  85 46     STA 46
3674  9D 05 C2  STA C205,X
3677  15 75     ORA 75,X
3679  91 19     STA (19),Y
367B  75 3C     ADC 3C,X
367D  72          ???
367E  2C 6F 08  BIT 086F
3681  CE 33 02  DEC 0233
3684  07          ???
3685  C4 BA     CPY BA
3687  50 A2     BVC 362B
3689  A5 DA     LDA DA
368B  EB          ???
368C  DA          ???
368D  E0 AA     CPX #AA
368F  68        PLA 
3690  EA        NOP 
3691  C6 B9     DEC B9
3693  66 13     ROR 13
3695  74          ???
3696  9B          ???
3697  A4 73     LDY 73
3699  8D FE 8E  STA 8EFE
369C  69 47     ADC #47
369E  93          ???
369F  10 16     BPL 36B7
36A1  69 24     ADC #24
36A3  9A        TXS 
36A4  64          ???
36A5  1C          ???
36A6  F6 7C     INC 7C,X
36A8  E0 54     CPX #54
36AA  4D 10 08  EOR 0810
36AD  5A          ???
36AE  3C          ???
36AF  13          ???
36B0  01 EE     ORA (EE,X)
36B2  37          ???
36B3  8D CD 22  STA 22CD
36B6  4C 05 14  JMP 1405
36B9  27          ???
36BA  F8        SED 
36BB  A9 31     LDA #31
36BD  20 AA 89  JSR 89AA
36C0  AC 32 D2  LDY D232
36C3  DB          ???
36C4  9E          ???
36C5  B9 A3 84  LDA 84A3,Y
36C8  72          ???
36C9  3D 89 08  AND 0889,X
36CC  CA        DEX 
36CD  0C          ???
36CE  04          ???
36CF  0A        ASL A
36D0  CF          ???
36D1  50 A1     BVC 3674
36D3  07          ???
36D4  D0 FB     BNE 36D1
36D6  61 27     ADC (27,X)
36D8  99 15 26  STA 2615,Y
36DB  5B          ???
36DC  8D 04 B9  STA B904
36DF  9F          ???
36E0  77          ???
36E1  EA        NOP 
36E2  2C 22 5B  BIT 5B22
36E5  2A        ROL A
36E6  A6 FB     LDX FB
36E8  05 17     ORA 17
36EA  6B          ???
36EB  98        TYA 
36EC  13          ???
36ED  1B          ???
36EE  BC 35 C5  LDY C535,X
36F1  93          ???
36F2  4C DD D3  JMP D3DD
36F5  39 58 98  AND 9858,Y
36F8  AB          ???
36F9  59 29 F6  EOR F629,Y
36FC  72          ???
36FD  6D 30 9A  ADC 9A30
3700  09 7B     ORA #7B
3702  0F          ???
3703  51 C8     EOR (C8),Y
3705  4C FE 9E  JMP 9EFE
3708  51 FA     EOR (FA),Y
370A  C1 3B     CMP (3B,X)
370C  97          ???
370D  41 8C     EOR (8C,X)
370F  45 D5     EOR D5
3711  02          ???
3712  2B          ???
3713  2B          ???
3714  12          ???
3715  6E 9B 97  ROR 979B
3718  C4 71     CPY 71
371A  78        SEI 
371B  6D 14 FE  ADC FE14
371E  64          ???
371F  95 32     STA 32,X
3721  70 57     BVS 377A
3723  8D 07 7E  STA 7E07
3726  07          ???
3727  74          ???
3728  A8        TAY 
3729  72          ???
372A  44          ???
372B  CD 20 AD  CMP AD20
372E  32          ???
372F  6B          ???
3730  AA        TAX 
3731  4D 31 32  EOR 3231
3734  3B          ???
3735  39 0B 77  AND 770B,Y
3738  C8        INY 
3739  8D 45 E6  STA E645
373C  0C          ???
373D  B5 2E     LDA 2E,X
373F  A8        TAY 
3740  18        CLC 
3741  F1 A1     SBC (A1),Y
3743  31 55     AND (55),Y
3745  19 4B 3C  ORA 3C4B,Y
3748  C0 06     CPY #06
374A  13          ???
374B  BF          ???
374C  30 E0     BMI 372E
374E  E6 8D     INC 8D
3750  59 85 E9  EOR E985,Y
3753  51 0A     EOR (0A),Y
3755  0B          ???
3756  75 E8     ADC E8,X
3758  C0 CA     CPY #CA
375A  08        PHP 
375B  EE E8 C0  INC C0E8
375E  9A        TXS 
375F  9A        TXS 
3760  12          ???
3761  95 6B     STA 6B,X
3763  2C 09 DB  BIT DB09
3766  A4 59     LDY 59
3768  24 AE     BIT AE
376A  38        SEC 
376B  D0 0E     BNE 377B
376D  6C 21 CA  JMP (CA21)
3770  86 E6     STX E6
3772  D1 B1     CMP (B1),Y
3774  3A          ???
3775  74          ???
3776  4A        LSR A
3777  10 E0     BPL 3759
3779  BA        TSX 
377A  66 22     ROR 22
377C  0C          ???
377D  DD 97 0E  CMP 0E97,X
3780  69 94     ADC #94
3782  5F          ???
3783  1A          ???
3784  91 DB     STA (DB),Y
3786  4B          ???
3787  0E 9B D5  ASL D59B
378A  8F          ???
378B  13          ???
378C  49 07     EOR #07
378E  35 8E     AND 8E,X
3790  AB          ???
3791  BA        TSX 
3792  6A        ROR A
3793  95 BF     STA BF,X
3795  18        CLC 
3796  5C          ???
3797  2C B2 8B  BIT 8BB2
379A  8B          ???
379B  3E 02 CE  ROL CE02,X
379E  28        PLP 
379F  08        PHP 
37A0  CB          ???
37A1  02          ???
37A2  8D 6B 79  STA 796B
37A5  37          ???
37A6  15 31     ORA 31,X
37A8  23          ???
37A9  A1 13     LDA (13,X)
37AB  3B          ???
37AC  22          ???
37AD  0E 55 26  ASL 2655
37B0  6F          ???
37B1  0A        ASL A
37B2  61 01     ADC (01,X)
37B4  31 33     AND (33),Y
37B6  B0 30     BCS 37E8
37B8  8D 58 69  STA 6958
37BB  06 D2     ASL D2
37BD  40        RTI 
37BE  9A        TXS 
37BF  5D CE 9D  EOR 9DCE,X
37C2  5A          ???
37C3  12          ???
37C4  37          ???
37C5  0D 56 4E  ORA 4E56
37C8  BA        TSX 
37C9  17          ???
37CA  E7          ???
37CB  0E 27 D6  ASL D627
37CE  23          ???
37CF  66 C4     ROR C4
37D1  40        RTI 
37D2  35 AD     AND AD,X
37D4  70 2B     BVS 3801
37D6  68        PLA 
37D7  B7          ???
37D8  B5 2E     LDA 2E,X
37DA  0D 49 28  ORA 2849
37DD  46 37     LSR 37
37DF  40        RTI 
37E0  96 EB     STX EB,Y
37E2  AE 63 12  LDX 1263
37E5  1F          ???
37E6  88        DEY 
37E7  34          ???
37E8  9C          ???
37E9  86 19     STX 19
37EB  EB          ???
37EC  B0 54     BCS 3842
37EE  4D 55 98  EOR 9855
37F1  75 51     ADC 51,X
37F3  E9 01     SBC #01
37F5  DE 08 95  DEC 9508,X
37F8  06 2E     ASL 2E
37FA  03          ???
37FB  2C D6 55  BIT 55D6
37FE  55 46     EOR 46,X
3800  79 22 5D  ADC 5D22,Y
3803  14          ???
3804  9D D9 AD  STA ADD9,X
3807  19 95 1D  ORA 1D95,Y
380A  10 02     BPL 380E
380C  9A        TXS 
380D  8D FE 16  STA 16FE
3810  8F          ???
3811  2E 04 93  ROL 9304
3814  7B          ???
3815  B9 10 D7  LDA D710,Y
3818  00        BRK 
3819  8D 17 DD  STA DD17
381C  95 37     STA 37,X
381E  A9 D7     LDA #D7
3820  12          ???
3821  55 56     EOR 56,X
3823  2D 08 AD  AND AD08
3826  A4 B8     LDY B8
3828  9B          ???
3829  10 D4     BPL 37FF
382B  FA          ???
382C  09 BA     ORA #BA
382E  64          ???
382F  90 ED     BCC 381E
3831  47          ???
3832  0D E5 A7  ORA A7E5
3835  EA        NOP 
3836  C2          ???
3837  B3          ???
3838  73          ???
3839  7B          ???
383A  9A        TXS 
383B  4D C9 06  EOR 06C9
383E  27          ???
383F  20 4D D6  JSR D64D
3842  37          ???
3843  07          ???
3844  51 6D     EOR (6D),Y
3846  22          ???
3847  8D 32 4E  STA 4E32
384A  A5 9A     LDA 9A
384C  EF          ???
384D  69 93     ADC #93
384F  D5 51     CMP 51,X
3851  1F          ???
3852  FE 0D 68  INC 680D,X
3855  0B          ???
3856  BA        TSX 
3857  B6 76     LDX 76,Y
3859  35 63     AND 63,X
385B  03          ???
385C  1D 2E A4  ORA A42E,X
385F  D6 AC     DEC AC,X
3861  E1 8C     SBC (8C,X)
3863  54          ???
3864  A6 BF     LDX BF
3866  8E 03 C3  STX C303
3869  53          ???
386A  48        PHA 
386B  57          ???
386C  0E 6A 06  ASL 066A
386F  73          ???
3870  8C 45 6A  STY 6A45
3873  10 E2     BPL 3857
3875  91 A2     STA (A2),Y
3877  5A          ???
3878  49 8E     EOR #8E
387A  6D 8D A4  ADC A48D
387D  0B          ???
387E  5F          ???
387F  C2          ???
3880  2C 7D 1F  BIT 1F7D
3883  BE 4C 55  LDX 554C,Y
3886  F6 16     INC 16,X
3888  D7          ???
3889  C4 6B     CPY 6B
388B  78        SEI 
388C  D4          ???
388D  87          ???
388E  0E 69 73  ASL 7369
3891  0C          ???
3892  AB          ???
3893  F3          ???
3894  0D 19 93  ORA 9319
3897  89          ???
3898  5A          ???
3899  96 0B     STX 0B,Y
389B  C3          ???
389C  14          ???
389D  8A        TXA 
389E  FF          ???
389F  5C          ???
38A0  38        SEC 
38A1  73          ???
38A2  70 8D     BVS 3831
38A4  DB          ???
38A5  07          ???
38A6  41 44     EOR (44,X)
38A8  F6 23     INC 23,X
38AA  E6 98     INC 98
38AC  17          ???
38AD  6A        ROR A
38AE  D2          ???
38AF  66 06     ROR 06
38B1  25 91     AND 91
38B3  33          ???
38B4  5F          ???
38B5  B9 99 76  LDA 7699,Y
38B8  C6 5A     DEC 5A
38BA  F5 52     SBC 52,X
38BC  E6 9E     INC 9E
38BE  AA        TAX 
38BF  94 77     STY 77,X
38C1  4E 1A 48  LSR 481A
38C4  E6 44     INC 44
38C6  9D 58 04  STA 0458,X
38C9  04          ???
38CA  97          ???
38CB  EA        NOP 
38CC  17          ???
38CD  D8        CLD 
38CE  09 70     ORA #70
38D0  E6 A0     INC A0
38D2  26 FE     ROL FE
38D4  A6 55     LDX 55
38D6  B3          ???
38D7  13          ???
38D8  64          ???
38D9  9D 81 9A  STA 9A81,X
38DC  4C 1B EA  JMP EA1B
38DF  92          ???
38E0  53          ???
38E1  1C          ???
38E2  04          ???
38E3  6A        ROR A
38E4  5C          ???
38E5  0D 15 ED  ORA ED15
38E8  33          ???
38E9  5B          ???
38EA  B6 45     LDX 45,Y
38EC  EA        NOP 
38ED  84 78     STY 78
38EF  26 71     ROL 71
38F1  8B          ???
38F2  AE B0 46  LDX 46B0
38F5  05 8C     ORA 8C
38F7  48        PHA 
38F8  A5 4B     LDA 4B
38FA  92          ???
38FB  32          ???
38FC  A7          ???
38FD  62          ???
38FE  EF          ???
38FF  6A        ROR A
3900  DC          ???
3901  D7          ???
3902  2F          ???
3903  F7          ???
3904  52          ???
3905  66 4A     ROR 4A
3907  17          ???
3908  93          ???
3909  4C C5 0A  JMP 0AC5
390C  92          ???
390D  F7          ???
390E  33          ???
390F  71 10     ADC (10),Y
3911  1D 0A 15  ORA 150A,X
3914  24 26     BIT 26
3916  AC 10 39  LDY 3910
3919  46 12     LSR 12
391B  C4 9C     CPY 9C
391D  75 53     ADC 53,X
391F  56 C6     LSR C6,X
3921  C1 15     CMP (15,X)
3923  A0 0D     LDY #0D
3925  B7          ???
3926  C2          ???
3927  2B          ???
3928  B4 AD     LDY AD,X
392A  E9 88     SBC #88
392C  90 0B     BCC 3939
392E  97          ???
392F  30 13     BMI 3944
3931  6F          ???
3932  0D 16 02  ORA 0216
3935  0D 12 8E  ORA 8E12
3938  68        PLA 
3939  ED 09 DD  SBC DD09
393C  7E A4 4A  ROR 4AA4,X
393F  D6 44     DEC 44,X
3941  E7          ???
3942  15 76     ORA 76,X
3944  30 13     BMI 3959
3946  BB          ???
3947  75 48     ADC 48,X
3949  52          ???
394A  5A          ???
394B  23          ???
394C  59 1D 18  EOR 181D,Y
394F  35 D5     AND D5,X
3951  91 6A     STA (6A),Y
3953  AC AC 41  LDY 41AC
3956  91 57     STA (57),Y
3958  34          ???
3959  A7          ???
395A  D6 35     DEC 35,X
395C  02          ???
395D  6A        ROR A
395E  40        RTI 
395F  8D 9B 12  STA 129B
3962  97          ???
3963  06 18     ASL 18
3965  25 05     AND 05
3967  5E 31 03  LSR 0331,X
396A  AF          ???
396B  47          ???
396C  46 6F     LSR 6F
396E  8B          ???
396F  B8        CLV 
3970  2C 6B 15  BIT 156B
3973  42          ???
3974  2A        ROL A
3975  10 3B     BPL 39B2
3977  51 B1     EOR (B1),Y
3979  A4 55     LDY 55
397B  45 99     EOR 99
397D  12          ???
397E  E1 DC     SBC (DC,X)
3980  AB          ???
3981  5A          ???
3982  99 80 8D  STA 8D80,Y
3985  BB          ???
3986  11 16     ORA (16),Y
3988  EB          ???
3989  47          ???
398A  F5 51     SBC 51,X
398C  6E 50 17  ROR 1750
398F  3B          ???
3990  1F          ???
3991  3A          ???
3992  5D C5 47  EOR 47C5,X
3995  6E 15 0C  ROR 0C15
3998  F1 07     SBC (07),Y
399A  BA        TSX 
399B  E8        INX 
399C  26 8D     ROL 8D
399E  DC          ???
399F  BA        TSX 
39A0  59 58 02  EOR 0258,Y
39A3  B8        CLV 
39A4  BE 89 0F  LDX 0F89,Y
39A7  A4 F6     LDY F6
39A9  C6 17     DEC 17
39AB  44          ???
39AC  1D 4B 99  ORA 994B,X
39AF  BC 51 F0  LDY F051,X
39B2  80          ???
39B3  8C B2 C6  STY C6B2
39B6  10 45     BPL 39FD
39B8  19 1A 14  ORA 141A,Y
39BB  61 0F     ADC (0F,X)
39BD  31 39     AND (39),Y
39BF  38        SEC 
39C0  36 8E     ROL 8E,X
39C2  5F          ???
39C3  01 40     ORA (40,X)
39C5  25 CA     AND CA
39C7  F1 AE     SBC (AE),Y
39C9  52          ???
39CA  4A        LSR A
39CB  A1 75     LDA (75,X)
39CD  71 4C     ADC (4C),Y
39CF  6C 06 4D  JMP (4D06)
39D2  14          ???
39D3  17          ???
39D4  FD DD 38  SBC 38DD,X
39D7  CE B3 6B  DEC 6BB3
39DA  CA        DEX 
39DB  BB          ???
39DC  C6 71     DEC 71
39DE  0A        ASL A
39DF  E4 4E     CPX 4E
39E1  44          ???
39E2  8B          ???
39E3  52          ???
39E4  E5 F8     SBC F8
39E6  46 EB     LSR EB
39E8  54          ???
39E9  48        PHA 
39EA  8B          ???
39EB  45 60     EOR 60
39ED  D2          ???
39EE  BD 4F 59  LDA 594F,X
39F1  4E 5E 46  LSR 465E
39F4  08        PHP 
39F5  E4 BA     CPX BA
39F7  70 E9     BVS 39E2
39F9  56 31     LSR 31,X
39FB  B1 8C     LDA (8C),Y
39FD  9E          ???
39FE  84 9D     STY 9D
3A00  90 C2     BCC 39C4
3A02  4B          ???
3A03  AC 8E 19  LDY 198E
3A06  B5 5E     LDA 5E,X
3A08  42          ???
3A09  A8        TAY 
3A0A  20 93 36  JSR 3693
3A0D  35 6C     AND 6C,X
3A0F  C4 34     CPY 34
3A11  0F          ???
3A12  07          ???
3A13  D7          ???
3A14  C4 66     CPY 66
3A16  08        PHP 
3A17  E5 DB     SBC DB
3A19  21 04     AND (04,X)
3A1B  22          ???
3A1C  36 20     ROL 20,X
3A1E  54          ???
3A1F  5E 16 43  LSR 4316,X
3A22  4A        LSR A
3A23  11 2E     ORA (2E),Y
3A25  E6 13     INC 13
3A27  03          ???
3A28  4E 51 D1  LSR D151
3A2B  76 AC     ROR AC,X
3A2D  E3          ???
3A2E  57          ???
3A2F  12          ???
3A30  53          ???
3A31  15 BA     ORA BA,X
3A33  20 91 53  JSR 5391
3A36  BA        TSX 
3A37  DA          ???
3A38  E8        INX 
3A39  68        PLA 
3A3A  0C          ???
3A3B  6D CA 06  ADC 06CA
3A3E  05 39     ORA 39
3A40  14          ???
3A41  56 62     LSR 62,X
3A43  EE A1 01  INC 01A1
3A46  A9 A6     LDA #A6
3A48  53          ???
3A49  AB          ???
3A4A  35 08     AND 08,X
3A4C  5E 94 D2  LSR D294,X
3A4F  0A        ASL A
3A50  A5 71     LDA 71
3A52  A9 02     LDA #02
3A54  7A          ???
3A55  B7          ???
3A56  2E 8C 30  ROL 308C
3A59  13          ???
3A5A  AF          ???
3A5B  97          ???
3A5C  52          ???
3A5D  52          ???
3A5E  05 56     ORA 56
3A60  64          ???
3A61  26 6E     ROL 6E
3A63  CA        DEX 
3A64  08        PHP 
3A65  19 F7 86  ORA 86F7,Y
3A68  13          ???
3A69  FC          ???
3A6A  A6 97     LDX 97
3A6C  59 ED 55  EOR 55ED,Y
3A6F  C4 41     CPY 41
3A71  EA        NOP 
3A72  7D 5B BA  ADC BA5B,X
3A75  84 D3     STY D3
3A77  4A        LSR A
3A78  88        DEY 
3A79  F4          ???
3A7A  2B          ???
3A7B  04          ???
3A7C  F5 12     SBC 12,X
3A7E  FA          ???
3A7F  71 5E     ADC (5E),Y
3A81  28        PLP 
3A82  95 A9     STA A9,X
3A84  80          ???
3A85  F4          ???
3A86  CB          ???
3A87  02          ???
3A88  4D C2 DA  EOR DAC2
3A8B  03          ???
3A8C  35 25     AND 25,X
3A8E  64          ???
3A8F  10 AA     BPL 3A3B
3A91  D1 98     CMP (98),Y
3A93  F4          ???
3A94  CE 5D 9A  DEC 9A5D
3A97  09 92     ORA #92
3A99  01 1A     ORA (1A,X)
3A9B  4B          ???
3A9C  0F          ???
3A9D  EA        NOP 
3A9E  09 D4     ORA #D4
3AA0  E9 CE     SBC #CE
3AA2  9D A9 B8  STA B8A9,X
3AA5  CD 9E D7  CMP D79E
3AA8  C1 D4     CMP (D4,X)
3AAA  82          ???
3AAB  0D 69 A8  ORA A869
3AAE  72          ???
3AAF  57          ???
3AB0  BB          ???
3AB1  99 09 4D  STA 4D09,Y
3AB4  E2          ???
3AB5  13          ???
3AB6  F5 81     SBC 81,X
3AB8  2E 41 82  ROL 8241
3ABB  04          ???
3ABC  9B          ???
3ABD  53          ???
3ABE  17          ???
3ABF  A5 D9     LDA D9
3AC1  41 12     EOR (12,X)
3AC3  14          ???
3AC4  DC          ???
3AC5  B8        CLV 
3AC6  4C 40 A9  JMP A940
3AC9  9A        TXS 
3ACA  90 69     BCC 3B35
3ACC  51 15     EOR (15),Y
3ACE  97          ???
3ACF  49 0E     EOR #0E
3AD1  AA        TAX 
3AD2  8D 09 37  STA 3709
3AD5  B3          ???
3AD6  D7          ???
3AD7  0A        ASL A
3AD8  81 4C     STA (4C,X)
3ADA  AA        TAX 
3ADB  2E AA 50  ROL 50AA
3ADE  33          ???
3ADF  05 4D     ORA 4D
3AE1  4B          ???
3AE2  53          ???
3AE3  0F          ???
3AE4  BF          ???
3AE5  21 01     AND (01,X)
3AE7  A7          ???
3AE8  04          ???
3AE9  55 2C     EOR 2C,X
3AEB  FC          ???
3AEC  C6 3C     DEC 3C
3AEE  D2          ???
3AEF  CA        DEX 
3AF0  0D C8 93  ORA 93C8
3AF3  7B          ???
3AF4  4A        LSR A
3AF5  0F          ???
3AF6  08        PHP 
3AF7  57          ???
3AF8  CD AE 14  CMP 14AE
3AFB  05 71     ORA 71
3AFD  2C 57 14  BIT 1457
3B00  88        DEY 
3B01  F5 0F     SBC 0F,X
3B03  02          ???
3B04  D7          ???
3B05  45 A6     EOR A6
3B07  0F          ???
3B08  54          ???
3B09  97          ???
3B0A  A9 AE     LDA #AE
3B0C  57          ???
3B0D  9B          ???
3B0E  45 18     EOR 18
3B10  03          ???
3B11  AF          ???
3B12  4B          ???
3B13  23          ???
3B14  D7          ???
3B15  2C 0C 6A  BIT 6A0C
3B18  0D 44 71  ORA 7144
3B1B  43          ???
3B1C  B5 39     LDA 39,X
3B1E  A0 EA     LDY #EA
3B20  14          ???
3B21  E9 08     SBC #08
3B23  52          ???
3B24  0C          ???
3B25  B1 04     LDA (04),Y
3B27  D4          ???
3B28  9E          ???
3B29  A0 DD     LDY #DD
3B2B  0F          ???
3B2C  93          ???
3B2D  7A          ???
3B2E  7D 49 48  ADC 4849,X
3B31  3D 2E 8E  AND 8E2E,X
3B34  60        RTS 
3B35  AA        TAX 
3B36  0E 4E 30  ASL 304E
3B39  EC E4 64  CPX 64E4
3B3C  52          ???
3B3D  AD 88 DB  LDA DB88
3B40  5A          ???
3B41  5C          ???
3B42  E8        INX 
3B43  CE 34 DB  DEC DB34
3B46  40        RTI 
3B47  58        CLI 
3B48  41 43     EOR (43,X)
3B4A  30 A8     BMI 3AF4
3B4C  40        RTI 
3B4D  26 92     ROL 92
3B4F  22          ???
3B50  97          ???
3B51  66 3E     ROR 3E
3B53  08        PHP 
3B54  19 2E DC  ORA DC2E,Y
3B57  18        CLC 
3B58  54          ???
3B59  52          ???
3B5A  5D 3C A0  EOR A03C,X
3B5D  C2          ???
3B5E  E6 43     INC 43
3B60  0C          ???
3B61  AC 53 21  LDY 2153
3B64  AB          ???
3B65  25 2B     AND 2B
3B67  0B          ???
3B68  4A        LSR A
3B69  52          ???
3B6A  36 17     ROL 17,X
3B6C  88        DEY 
3B6D  4F          ???
3B6E  25 06     AND 06
3B70  96 E9     STX E9,Y
3B72  01 AF     ORA (AF,X)
3B74  14          ???
3B75  CF          ???
3B76  1D 7C BA  ORA BA7C,X
3B79  09 C2     ORA #C2
3B7B  06 11     ASL 11
3B7D  85 46     STA 46
3B7F  54          ???
3B80  4E 47 55  LSR 5547
3B83  53          ???
3B84  5E 09 0E  LSR 0E09,X
3B87  44          ???
3B88  74          ???
3B89  0D 15 D4  ORA D415
3B8C  86 76     STX 76
3B8E  D5 14     CMP 14,X
3B90  69 10     ADC #10
3B92  4E 63 02  LSR 0263
3B95  19 65 0A  ORA 0A65,Y
3B98  2A        ROL A
3B99  52          ???
3B9A  4A        LSR A
3B9B  45 44     EOR 44
3B9D  49 BC     EOR #BC
3B9F  00        BRK 
3BA0  EA        NOP 
3BA1  53          ???
3BA2  95 19     STA 19,X
3BA4  AE 7A 05  LDX 057A
3BA7  0D EA 15  ORA 15EA
3BAA  56 A5     LSR A5,X
3BAC  50 54     BVC 3C02
3BAE  2F          ???
3BAF  4E 6B 54  LSR 546B
3BB2  53          ???
3BB3  43          ???
3BB4  E5 D4     SBC D4
3BB6  44          ???
3BB7  12          ???
3BB8  CB          ???
3BB9  0C          ???
3BBA  19 2D A7  ORA A72D,Y
3BBD  6A        ROR A
3BBE  CA        DEX 
3BBF  D1 0A     CMP (0A),Y
3BC1  34          ???
3BC2  C8        INY 
3BC3  1C          ???
3BC4  48        PHA 
3BC5  A5 10     LDA 10
3BC7  37          ???
3BC8  AC 2F 2A  LDY 2A2F
3BCB  35 38     AND 38,X
3BCD  31 2E     AND (2E),Y
3BCF  30 46     BMI 3C17
3BD1  76 65     ROR 65,X
3BD3  32          ???
3BD4  48        PHA 
3BD5  06 2F     ASL 2F
3BD7  34          ???
3BD8  C0 30     CPY #30
3BDA  55 20     EOR 20,X
3BDC  8C 41 12  STY 1241
3BDF  E0 4D     CPX #4D
3BE1  D5 05     CMP 05,X
3BE3  48        PHA 
3BE4  44          ???
3BE5  76 4E     ROR 4E,X
3BE7  35 1A     AND 1A,X
3BE9  09 16     ORA #16
3BEB  AC BE 77  LDY 77BE
3BEE  55 8C     EOR 8C,X
3BF0  09 20     ORA #20
3BF2  70 57     BVS 3C4B
3BF4  15 2E     ORA 2E,X
3BF6  4E 4F 53  LSR 534F
3BF9  54          ???
3BFA  FE 4C 47  INC 474C,X
3BFD  49 41     EOR #41
3BFF  2F          ???
3C00  43          ???
3C01  C5 36     CMP 36
3C03  34          ???
3C04  2E 4F 52  ROL 524F
3C07  47          ???
3C08  8E 8C FF  STX FF8C
3C0B  27          ???
3C0C  42          ???
3C0D  15 6B     ORA 6B,X
3C0F  07          ???
3C10  06 09     ASL 09
3C12  18        CLC 
3C13  F1 2F     SBC (2F),Y
3C15  15 46     ORA 46,X
3C17  92          ???
3C18  01 90     ORA (90,X)
3C1A  17          ???
3C1B  13          ???
3C1C  3A          ???
3C1D  7A          ???
3C1E  46 35     LSR 35
3C20  43          ???
3C21  4D 89 43  EOR 4389
3C24  54          ???
3C25  91 BF     STA (BF),Y
3C27  2C 20 39  BIT 3920
3C2A  6D 31 68  ADC 6831
3C2D  B3          ???
3C2E  A1 22     LDA (22,X)
3C30  62          ???
3C31  4E A1 2B  LSR 2BA1
3C34  52          ???
3C35  D5 41     CMP 41,X
3C37  4D B5 F1  EOR F1B5
3C3A  44          ???
3C3B  9C          ???
3C3C  06 56     ASL 56
3C3E  96 4A     STX 4A,Y
3C40  28        PLP 
3C41  EB          ???
3C42  84 66     STY 66
3C44  A1 B3     LDA (B3,X)
3C46  AB          ???
3C47  65 01     ADC 01
3C49  C6 44     DEC 44
3C4B  FD 4C 9A  SBC 9A4C,X
3C4E  B7          ???
3C4F  9C          ???
3C50  88        DEY 
3C51  4A        LSR A
3C52  15 C8     ORA C8,X
3C54  6B          ???
3C55  A1 15     LDA (15,X)
3C57  13          ???
3C58  B5 D8     LDA D8,X
3C5A  C1 83     CMP (83,X)
3C5C  6A        ROR A
3C5D  0C          ???
3C5E  F2          ???
3C5F  4D 26 04  EOR 0426
3C62  91 B3     STA (B3),Y
3C64  44          ???
3C65  AE 91 AA  LDX AA91
3C68  26 76     ROL 76
3C6A  11 20     ORA (20),Y
3C6C  50 19     BVC 3C87
3C6E  D2          ???
3C6F  B9 C4 35  LDA 35C4,Y
3C72  0C          ???
3C73  14          ???
3C74  77          ???
3C75  78        SEI 
3C76  60        RTS 
3C77  EA        NOP 
3C78  12          ???
3C79  F2          ???
3C7A  11 09     ORA (09),Y
3C7C  5E D8 C4  LSR C4D8,X
3C7F  5A          ???
3C80  64          ???
3C81  92          ???
3C82  12          ???
3C83  B7          ???
3C84  D5 D7     CMP D7,X
3C86  30 3A     BMI 3CC2
3C88  1A          ???
3C89  47          ???
3C8A  25 98     AND 98
3C8C  98        TYA 
3C8D  26 61     ROL 61
3C8F  0A        ASL A
3C90  B5 E3     LDA E3,X
3C92  13          ???
3C93  0C          ???
3C94  B5 2A     LDA 2A,X
3C96  15 10     ORA 10,X
3C98  D7          ???
3C99  98        TYA 
3C9A  5D 0A 2A  EOR 2A0A,X
3C9D  AC DA A4  LDY A4DA
3CA0  D6 B5     DEC B5,X
3CA2  03          ???
3CA3  59 47 C6  EOR C647,Y
3CA6  28        PLP 
3CA7  43          ???
3CA8  FA          ???
3CA9  03          ???
3CAA  AA        TAX 
3CAB  0A        ASL A
3CAC  84 38     STY 38
3CAE  32          ???
3CAF  57          ???
3CB0  60        RTS 
3CB1  30 49     BMI 3CFC
3CB3  39 05 04  AND 0405,Y
3CB6  BE 0D 0A  LDX 0A0D,Y
3CB9  8C 44 14  STY 1444
3CBC  86 05     STX 05
3CBE  75 2F     ADC 2F,X
3CC0  04          ???
3CC1  E2          ???
3CC2  0B          ???
3CC3  20 57 1D  JSR 1D57
3CC6  46 5A     LSR 5A
3CC8  29 95     AND #95
3CCA  1C          ???
3CCB  83          ???
3CCC  13          ???
3CCD  EA        NOP 
3CCE  13          ???
3CCF  AC A7 0C  LDY 0CA7
3CD2  01 04     ORA (04,X)
3CD4  19 78 8A  ORA 8A78,Y
3CD7  52          ???
3CD8  C2          ???
3CD9  09 40     ORA #40
3CDB  B1 00     LDA (00),Y
3CDD  D0 E9     BNE 3CC8
3CDF  C4 0E     CPY 0E
3CE1  A5 1D     LDA 1D
3CE3  12          ???
3CE4  02          ???
3CE5  0D 29 51  ORA 5129
3CE8  13          ???
3CE9  03          ???
3CEA  2B          ???
3CEB  A2 04     LDX #04
3CED  D7          ???
3CEE  9D 48 AA  STA AA48,X
3CF1  4A        LSR A
3CF2  B3          ???
3CF3  69 03     ADC #03
3CF5  96 06     STX 06,Y
3CF7  37          ???
3CF8  2D 99 CB  AND CB99
3CFB  23          ???
3CFC  42          ???
3CFD  16 C7     ASL C7,X
3CFF  56 A0     LSR A0,X
3D01  D7          ???
3D02  55 EC     EOR EC,X
3D04  30 2B     BMI 3D31
3D06  61 42     ADC (42,X)
3D08  A8        TAY 
3D09  49 06     EOR #06
3D0B  BB          ???
3D0C  83          ???
3D0D  12          ???
3D0E  AD C3 10  LDA 10C3
3D11  0C          ???
3D12  D6 15     DEC 15,X
3D14  17          ???
3D15  01 6A     ORA (6A,X)
3D17  82          ???
3D18  43          ???
3D19  15 0D     ORA 0D,X
3D1B  02          ???
3D1C  78        SEI 
3D1D  D5 01     CMP 01,X
3D1F  2C 6C 83  BIT 836C
3D22  4D 17 1D  EOR 1D17
3D25  BE A7 15  LDX 15A7,Y
3D28  B2          ???
3D29  0F          ???
3D2A  35 90     AND 90,X
3D2C  B5 01     LDA 01,X
3D2E  41 62     EOR (62,X)
3D30  20 F4 0C  JSR 0CF4
3D33  04          ???
3D34  AC 05 12  LDY 1205
3D37  96 05     STX 05,Y
3D39  AD 94 72  LDA 7294
3D3C  91 34     STA (34),Y
3D3E  D2          ???
3D3F  07          ???
3D40  74          ???
3D41  E4 6F     CPX 6F
3D43  4D 1F 2D  EOR 2D1F
3D46  0D 57 25  ORA 2557
3D49  E6 3B     INC 3B
3D4B  15 24     ORA 24,X
3D4D  23          ???
3D4E  B4 B5     LDY B5,X
3D50  72          ???
3D51  81 11     STA (11,X)
3D53  35 15     AND 15,X
3D55  98        TYA 
3D56  A5 2D     LDA 2D
3D58  02          ???
3D59  8D 7A EF  STA EF7A
3D5C  06 E1     ASL E1
3D5E  57          ???
3D5F  9A        TXS 
3D60  13          ???
3D61  05 01     ORA 01
3D63  07          ???
3D64  05 E4     ORA E4
3D66  22          ???
3D67  19 B7 4D  ORA 4DB7,Y
3D6A  0D 08 B4  ORA B408
3D6D  20 49 22  JSR 2249
3D70  95 9A     STA 9A,X
3D72  09 07     ORA #07
3D74  08        PHP 
3D75  BE CA C5  LDX C5CA,Y
3D78  0F          ???
3D79  10 0B     BPL 3D86
3D7B  04          ???
3D7C  65 51     ADC 51
3D7E  4E 47 3C  LSR 3C47
3D81  14          ???
3D82  1D B2 13  ORA 13B2,X
3D85  2E 1B 54  ROL 541B
3D88  25 C1     AND C1
3D8A  14          ???
3D8B  20 08 01  JSR 0108
3D8E  5E 10 0B  LSR 0B10,X
3D91  C2          ???
3D92  13          ???
3D93  B8        CLV 
3D94  83          ???
3D95  17          ???
3D96  08        PHP 
3D97  78        SEI 
3D98  D5 D2     CMP D2,X
3D9A  10 15     BPL 3DB1
3D9C  B4 09     LDY 09,X
3D9E  13          ???
3D9F  5E 0C 2A  LSR 2A0C,X
3DA2  17          ???
3DA3  2A        ROL A
3DA4  2C 15 0D  BIT 0D15
3DA7  0F          ???
3DA8  4B          ???
3DA9  05 25     ORA 25
3DAB  19 A8 A2  ORA A2A8,Y
3DAE  56 03     LSR 03,X
3DB0  2B          ???
3DB1  4B          ???
3DB2  E5 8D     SBC 8D
3DB4  41 30     EOR (30,X)
3DB6  A0 06     LDY #06
3DB8  0F          ???
3DB9  5E 04 2C  LSR 2C04,X
3DBC  8A        TXA 
3DBD  12          ???
3DBE  05 64     ORA 64
3DC0  82          ???
3DC1  09 0E     ORA #0E
3DC3  07          ???
3DC4  BC 26 20  LDY 2026,X
3DC7  14          ???
3DC8  0F          ???
3DC9  15 12     ORA 12,X
3DCB  5F          ???
3DCC  01 C5     ORA (C5,X)
3DCE  0D 05 0E  ORA 0E05
3DD1  14          ???
3DD2  2E 8E 8F  ROL 8F8E
3DD5  FF          ???
3DD6  11 10     ORA (10),Y
3DD8  3D CC D5  AND D5CC,X
3DDB  44          ???
3DDC  44          ???
3DDD  59 99 98  EOR 9899,Y
3DE0  E7          ???
3DE1  AA        TAX 
3DE2  AB          ???
3DE3  33          ???
3DE4  32          ???
3DE5  23          ???
3DE6  CC CD 0D  CPY 0DCD
3DE9  59 00 90  EOR 9000,Y
3DEC  91 11     STA (11),Y
3DEE  01 84     ORA (84,X)
3DF0  58        CLI 
3DF1  AC E8 A9  LDY A9E8
3DF4  00        BRK 
3DF5  85 FC     STA FC
3DF7  85 FB     STA FB
3DF9  E0 01     CPX #01
3DFB  90 28     BCC 3E25
3DFD  A5 FD     LDA FD
3DFF  4A        LSR A
3E00  D0 18     BNE 3E1A
3E02  AD 23 01  LDA 0123
3E05  D0 0A     BNE 3E11
3E07  C6 01     DEC 01
3E09  8E E7 DB  STX DBE7
3E0C  E6 01     INC 01
3E0E  CE 24 01  DEC 0124
3E11  CE 23 01  DEC 0123
3E14  AD EF 3D  LDA 3DEF
3E17  90 1B     BCC 3E34
3E19  6A        ROR A
3E1A  26 FC     ROL FC
3E1C  26 FB     ROL FB
3E1E  CA        DEX 
3E1F  D0 DE     BNE 3DFF
3E21  85 FD     STA FD
3E23  A5 FC     LDA FC
3E25  60        RTS 
3E26  C6 01     DEC 01
3E28  58        CLI 
3E29  4C 00 3D  JMP 3D00
3E2C  CA        DEX 
3E2D  C6 FF     DEC FF
3E2F  C6 AF     DEC AF
3E31  88        DEY 
3E32  B1 AE     LDA (AE),Y
3E34  91 FE     STA (FE),Y
3E36  98        TYA 
3E37  D0 F8     BNE 3E31
3E39  8A        TXA 
3E3A  D0 F0     BNE 3E2C
3E3C  20 00 01  JSR 0100
3E3F  F0 0A     BEQ 3E4B
3E41  A5 FE     LDA FE
3E43  D0 02     BNE 3E47
3E45  C6 FF     DEC FF
3E47  C6 FE     DEC FE
3E49  90 B7     BCC 3E02
3E4B  C8        INY 
3E4C  20 00 01  JSR 0100
3E4F  F0 FA     BEQ 3E4B
3E51  C0 11     CPY #11
3E53  B0 D1     BCS 3E26
3E55  BE 33 03  LDX 0333,Y
3E58  20 01 01  JSR 0101
3E5B  79 67 03  ADC 0367,Y
3E5E  85 A7     STA A7
3E60  A5 FB     LDA FB
3E62  79 9B 03  ADC 039B,Y
3E65  48        PHA 
3E66  D0 06     BNE 3E6E
3E68  A4 A7     LDY A7
3E6A  C0 04     CPY #04
3E6C  90 02     BCC 3E70
3E6E  A0 03     LDY #03
3E70  BE B3 01  LDX 01B3,Y
3E73  20 01 01  JSR 0101
3E76  79 B6 01  ADC 01B6,Y
3E79  A8        TAY 
3E7A  38        SEC 
3E7B  A5 FE     LDA FE
3E7D  E5 A7     SBC A7
3E7F  85 FE     STA FE
