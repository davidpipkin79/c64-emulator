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
0811  BD 8A 27  LDA 278A,X
0814  9D FC 00  STA 00FC,X
0817  CA        DEX 
0818  D0 F7     BNE 0811
081A  4C 48 28  JMP 2848
081D  80          ???
081E  00        BRK 
081F  A2 0E     LDX #0E
0821  3A          ???
0822  D5 4E     CMP 4E,X
0824  08        PHP 
0825  D6 8B     DEC 8B,X
0827  F9 0F 1E  SBC 1E0F,Y
082A  0F          ???
082B  0B          ???
082C  10 DC     BPL 080A
082E  D1 8C     CMP (8C),Y
0830  03          ???
0831  B9 C4 68  LDA 68C4,Y
0834  0B          ???
0835  11 4C     ORA (4C),Y
0837  0D 78 2B  ORA 2B78
083A  35 CE     AND CE,X
083C  36 EC     ROL EC,X
083E  D3          ???
083F  CE 76 46  DEC 4676
0842  F3          ???
0843  BE D8 0F  LDX 0FD8,Y
0846  C6 F0     DEC F0
0848  EB          ???
0849  9C          ???
084A  53          ???
084B  0D 0A 43  ORA 430A
084E  90 E1     BCC 0831
0850  FB          ???
0851  75 E7     ADC E7,X
0853  83          ???
0854  E1 96     SBC (96,X)
0856  0E 16 87  ASL 8716
0859  D7          ???
085A  C3          ???
085B  B7          ???
085C  94 76     STY 76,X
085E  9C          ???
085F  FA          ???
0860  4F          ???
0861  25 3B     AND 3B
0863  A5 4A     LDA 4A
0865  D4          ???
0866  10 1E     BPL 0886
0868  49 B5     EOR #B5
086A  64          ???
086B  C9 01     CMP #01
086D  B9 B7 10  LDA 10B7,Y
0870  B4 6B     LDY 6B,X
0872  EF          ???
0873  84 CF     STY CF
0875  79 A3 06  ADC 06A3,Y
0878  89          ???
0879  09 0F     ORA #0F
087B  87          ???
087C  F6 B5     INC B5,X
087E  25 35     AND 35
0880  77          ???
0881  8F          ???
0882  64          ???
0883  60        RTS 
0884  A6 C4     LDX C4
0886  25 86     AND 86
0888  8B          ???
0889  AD 2A 5A  LDA 5A2A
088C  4C 3D B9  JMP B93D
088F  03          ???
0890  15 F5     ORA F5,X
0892  7A          ???
0893  D0 08     BNE 089D
0895  7B          ???
0896  D9 AF 49  CMP 49AF,Y
0899  D0 3C     BNE 08D7
089B  18        CLC 
089C  62          ???
089D  98        TYA 
089E  A1 91     LDA (91,X)
08A0  68        PLA 
08A1  B1 01     LDA (01),Y
08A3  B0 AD     BCS 0852
08A5  37          ???
08A6  15 63     ORA 63,X
08A8  F0 AE     BEQ 0858
08AA  10 0E     BPL 08BA
08AC  DC          ???
08AD  B1 D1     LDA (D1),Y
08AF  00        BRK 
08B0  DB          ???
08B1  09 9D     ORA #9D
08B3  37          ???
08B4  1B          ???
08B5  2A        ROL A
08B6  B6 38     LDX 38,Y
08B8  1C          ???
08B9  FD D5 38  SBC 38D5,X
08BC  F0 12     BEQ 08D0
08BE  3D 14 62  AND 6214,X
08C1  5B          ???
08C2  6C 76 1C  JMP (1C76)
08C5  4E 1C 98  LSR 981C
08C8  77          ???
08C9  DC          ???
08CA  DA          ???
08CB  78        SEI 
08CC  4E 3B 8F  LSR 8F3B
08CF  D3          ???
08D0  B5 39     LDA 39,X
08D2  9E          ???
08D3  38        SEC 
08D4  6E F6 E9  ROR E9F6
08D7  14          ???
08D8  FE EE 01  INC 01EE,X
08DB  F9 95 C2  SBC C295,Y
08DE  16 82     ASL 82,X
08E0  51 71     EOR (71),Y
08E2  68        PLA 
08E3  59 80 18  EOR 1880,Y
08E6  59 02 61  EOR 6102,Y
08E9  4C 71 13  JMP 1371
08EC  14          ???
08ED  C9 18     CMP #18
08EF  75 E9     ADC E9,X
08F1  D0 3A     BNE 092D
08F3  67          ???
08F4  1A          ???
08F5  A6 B6     LDX B6
08F7  D6 8C     DEC 8C,X
08F9  7D 32 B5  ADC B532,X
08FC  21 44     AND (44,X)
08FE  0E 22 2C  ASL 2C22
0901  E8        INX 
0902  23          ???
0903  F2          ???
0904  1F          ???
0905  8E F8 D0  STX D0F8
0908  06 6E     ASL 6E
090A  E4 A8     CPX A8
090C  55 C0     EOR C0,X
090E  90 A3     BCC 08B3
0910  09 97     ORA #97
0912  8C A2 D2  STY D2A2
0915  10 08     BPL 091F
0917  37          ???
0918  E9 A0     SBC #A0
091A  DB          ???
091B  24 57     BIT 57
091D  74          ???
091E  95 5A     STA 5A,X
0920  C8        INY 
0921  94 23     STY 23,X
0923  F5 8A     SBC 8A,X
0925  AD 1B 69  LDA 691B
0928  13          ???
0929  B9 38 16  LDA 1638,Y
092C  85 8E     STA 8E
092E  B4 37     LDY 37,X
0930  7F          ???
0931  96 5F     STX 5F,Y
0933  89          ???
0934  22          ???
0935  5C          ???
0936  F3          ???
0937  05 18     ORA 18
0939  6D 66 87  ADC 8766
093C  08        PHP 
093D  9F          ???
093E  19 04 75  ORA 7504,Y
0941  C6 AB     DEC AB
0943  3F          ???
0944  41 1A     EOR (1A,X)
0946  29 DA     AND #DA
0948  EA        NOP 
0949  39 A2 0C  AND 0CA2,Y
094C  79 5D E0  ADC E05D,Y
094F  21 0B     AND (0B,X)
0951  3D F0 04  AND 04F0,X
0954  69 3F     ADC #3F
0956  F6 0F     INC 0F,X
0958  BE 90 0A  LDX 0A90,Y
095B  A5 61     LDA 61
095D  0B          ???
095E  87          ???
095F  07          ???
0960  11 1B     ORA (1B),Y
0962  27          ???
0963  85 35     STA 35
0965  AE 45 F0  LDX F045
0968  90 06     BCC 0970
096A  EE 0C 78  INC 780C
096D  A9 C7     LDA #C7
096F  48        PHA 
0970  C8        INY 
0971  5B          ???
0972  B1 8D     LDA (8D),Y
0974  E3          ???
0975  01 6B     ORA (6B,X)
0977  94 37     STY 37,X
0979  68        PLA 
097A  C9 5F     CMP #5F
097C  7D 39 CC  ADC CC39,X
097F  F0 0F     BEQ 0990
0981  A8        TAY 
0982  A7          ???
0983  B6 72     LDX 72,Y
0985  49 24     EOR #24
0987  E4 3A     CPX 3A
0989  98        TYA 
098A  F7          ???
098B  0C          ???
098C  55 26     EOR 26,X
098E  10 03     BPL 0993
0990  71 B6     ADC (B6),Y
0992  1D 90 2C  ORA 2C90,X
0995  F4          ???
0996  58        CLI 
0997  7A          ???
0998  D6 3B     DEC 3B,X
099A  B4 39     LDY 39,X
099C  B9 1D FF  LDA FF1D,Y
099F  4E C0 E7  LSR E7C0
09A2  11 C0     ORA (C0),Y
09A4  DB          ???
09A5  20 F0 0C  JSR 0CF0
09A8  1C          ???
09A9  FD C6 01  SBC 01C6,X
09AC  B8        CLV 
09AD  C8        INY 
09AE  0F          ???
09AF  9D 05 77  STA 7705,X
09B2  48        PHA 
09B3  77          ???
09B4  61 43     ADC (43,X)
09B6  E0 0A     CPX #0A
09B8  46 79     LSR 79
09BA  AC 34 4C  LDY 4C34
09BD  35 0C     AND 0C,X
09BF  DD 38 30  CMP 3038,X
09C2  0B          ???
09C3  5B          ???
09C4  15 89     ORA 89,X
09C6  9D 06 D4  STA D406,X
09C9  FF          ???
09CA  52          ???
09CB  78        SEI 
09CC  5F          ???
09CD  3A          ???
09CE  F0 23     BEQ 09F3
09D0  AB          ???
09D1  95 C6     STA C6,X
09D3  07          ???
09D4  D2          ???
09D5  FE 2C A9  INC A92C,X
09D8  FF          ???
09D9  BE 77 F7  LDX F777,Y
09DC  61 30     ADC (30,X)
09DE  B7          ???
09DF  B9 E9 DB  LDA DBE9,Y
09E2  12          ???
09E3  8C 0A A8  STY A80A
09E6  59 47 B5  EOR B547,Y
09E9  CC 56 22  CPY 2256
09EC  63          ???
09ED  41 27     EOR (27,X)
09EF  95 62     STA 62,X
09F1  B0 1A     BCS 0A0D
09F3  B9 34 D0  LDA D034,Y
09F6  05 6D     ORA 6D
09F8  7F          ???
09F9  A3          ???
09FA  B9 8E 77  LDA 778E,Y
09FD  BA        TSX 
09FE  34          ???
09FF  D8        CLD 
0A00  4B          ???
0A01  1D 44 96  ORA 9644,X
0A04  4C D1 7F  JMP 7FD1
0A07  B5 B5     LDA B5,X
0A09  1B          ???
0A0A  58        CLI 
0A0B  65 D0     ADC D0
0A0D  6A        ROR A
0A0E  F0 59     BEQ 0A69
0A10  B9 8F 15  LDA 158F,Y
0A13  FF          ???
0A14  3C          ???
0A15  7F          ???
0A16  90 4E     BCC 0A66
0A18  C9 F8     CMP #F8
0A1A  FF          ???
0A1B  CD 8D 21  CMP 218D
0A1E  CC A5 DD  CPY DDA5
0A21  68        PLA 
0A22  AA        TAX 
0A23  36 B9     ROL B9,X
0A25  AB          ???
0A26  A6 8E     LDX 8E
0A28  6F          ???
0A29  AE C1 E8  LDX E8C1
0A2C  02          ???
0A2D  A9 B0     LDA #B0
0A2F  8D 96 FF  STA FF96
0A32  F6 62     INC 62,X
0A34  5B          ???
0A35  CC 12 AA  CPY AA12
0A38  B5 88     LDA 88,X
0A3A  88        DEY 
0A3B  69 28     ADC #28
0A3D  A0 94     LDY #94
0A3F  16 6A     ASL 6A,X
0A41  27          ???
0A42  8D 66 6E  STA 6E66
0A45  D9 A7 0C  CMP 0CA7,Y
0A48  E9 D5     SBC #D5
0A4A  23          ???
0A4B  90 5F     BCC 0AAC
0A4D  C9 7A     CMP #7A
0A4F  B0 A6     BCS 09F7
0A51  F6 64     INC 64,X
0A53  D7          ???
0A54  18        CLC 
0A55  B9 5D 91  LDA 915D,Y
0A58  15 10     ORA 10,X
0A5A  FD A1 D6  SBC D6A1,X
0A5D  62          ???
0A5E  3D 04 DD  AND DD04,X
0A61  26 61     ROL 61
0A63  2D 63 5A  AND 5A63
0A66  02          ???
0A67  D2          ???
0A68  64          ???
0A69  9D 6D 4C  STA 4C6D,X
0A6C  B8        CLV 
0A6D  5B          ???
0A6E  3A          ???
0A6F  5F          ???
0A70  53          ???
0A71  12          ???
0A72  30 D1     BMI 0A45
0A74  09 80     ORA #80
0A76  74          ???
0A77  6C 87 4C  JMP (4C87)
0A7A  64          ???
0A7B  13          ???
0A7C  37          ???
0A7D  5C          ???
0A7E  E0 1B     CPX #1B
0A80  90 38     BCC 0ABA
0A82  EB          ???
0A83  A8        TAY 
0A84  10 90     BPL 0A16
0A86  0C          ???
0A87  67          ???
0A88  B4 B0     LDY B0,X
0A8A  56 B0     LSR B0,X
0A8C  D0 0A     BNE 0A98
0A8E  F8        SED 
0A8F  4B          ???
0A90  A0 90     LDY #90
0A92  8C 07 44  STY 4407
0A95  13          ???
0A96  E4 62     CPX 62
0A98  50 E9     BVC 0A83
0A9A  DB          ???
0A9B  18        CLC 
0A9C  21 A3     AND (A3,X)
0A9E  45 2C     EOR 2C
0AA0  FB          ???
0AA1  79 65 48  ADC 4865,Y
0AA4  A9 38     LDA #38
0AA6  B4 76     LDY 76,X
0AA8  A5 8D     LDA 8D
0AAA  FD 5D 68  SBC 685D,X
0AAD  59 F9 EF  EOR EFF9,Y
0AB0  A3          ???
0AB1  94 50     STY 50,X
0AB3  77          ???
0AB4  8E 74 8E  STX 8E74
0AB7  24 07     BIT 07
0AB9  D1 19     CMP (19),Y
0ABB  F0 41     BEQ 0AFE
0ABD  0A        ASL A
0ABE  75 AF     ADC AF,X
0AC0  4C A8 B5  JMP B5A8
0AC3  F4          ???
0AC4  3C          ???
0AC5  59 0A C0  EOR C00A,Y
0AC8  36 56     ROL 56,X
0ACA  A4 AD     LDY AD
0ACC  80          ???
0ACD  B2          ???
0ACE  1A          ???
0ACF  4D 15 6A  EOR 6A15
0AD2  A1 B1     LDA (B1,X)
0AD4  A3          ???
0AD5  AD 5B 4F  LDA 4F5B
0AD8  B9 A5 15  LDA 15A5,Y
0ADB  3C          ???
0ADC  1D 86 59  ORA 5986,X
0ADF  F8        SED 
0AE0  DC          ???
0AE1  5E B5 50  LSR 50B5,X
0AE4  FF          ???
0AE5  18        CLC 
0AE6  65 8D     ADC 8D
0AE8  7F          ???
0AE9  60        RTS 
0AEA  D8        CLD 
0AEB  02          ???
0AEC  29 1F     AND #1F
0AEE  A8        TAY 
0AEF  DE C8 90  DEC 90C8,X
0AF2  05 C6     ORA C6
0AF4  4B          ???
0AF5  D5 A1     CMP A1,X
0AF7  95 8F     STA 8F,X
0AF9  EA        NOP 
0AFA  D1 13     CMP (13),Y
0AFC  BB          ???
0AFD  B5 F8     LDA F8,X
0AFF  06 D6     ASL D6
0B01  4F          ???
0B02  D0 02     BNE 0B06
0B04  F6 FE     INC FE,X
0B06  71 8C     ADC (8C),Y
0B08  B4 4E     LDY 4E,X
0B0A  30 0C     BMI 0B18
0B0C  F9 2C 33  SBC 332C,Y
0B0F  75 0D     ADC 0D,X
0B11  4C C0 13  JMP 13C0
0B14  38        SEC 
0B15  6F          ???
0B16  4A        LSR A
0B17  EB          ???
0B18  D7          ???
0B19  E5 F5     SBC F5
0B1B  4B          ???
0B1C  D3          ???
0B1D  EC 88 98  CPX 9888
0B20  FB          ???
0B21  85 24     STA 24
0B23  CA        DEX 
0B24  50 04     BVC 0B2A
0B26  5B          ???
0B27  49 7F     EOR #7F
0B29  F7          ???
0B2A  4D 95 4E  EOR 4E95
0B2D  76 5F     ROR 5F,X
0B2F  AD FE 13  LDA 13FE
0B32  B5 60     LDA 60,X
0B34  8D 06 14  STA 1406
0B37  7F          ???
0B38  B4 D6     LDY D6,X
0B3A  B2          ???
0B3B  06 09     ASL 09
0B3D  09 E2     ORA #E2
0B3F  54          ???
0B40  26 C9     ROL C9
0B42  8D 44 2D  STA 2D44
0B45  95 25     STA 25,X
0B47  65 6B     ADC 6B
0B49  15 B6     ORA B6,X
0B4B  18        CLC 
0B4C  75 50     ADC 50,X
0B4E  9B          ???
0B4F  29 7F     AND #7F
0B51  A8        TAY 
0B52  E9 36     SBC #36
0B54  A9 18     LDA #18
0B56  A5 23     LDA 23
0B58  A8        TAY 
0B59  B9 96 14  LDA 1496,Y
0B5C  69 00     ADC #00
0B5E  DF          ???
0B5F  01 4D     ORA (4D,X)
0B61  F6 4D     INC 4D,X
0B63  26 86     ROL 86
0B65  77          ???
0B66  35 25     AND 25,X
0B68  9D 04 D4  STA D404,X
0B6B  B5 20     LDA 20,X
0B6D  FF          ???
0B6E  AA        TAX 
0B6F  30 03     BMI 0B74
0B71  4C FE C5  JMP C5FE
0B74  CE 73 41  DEC 4173
0B77  4E 36 96  LSR 9636
0B7A  3D C6 44  AND 44C6,X
0B7D  5F          ???
0B7E  02          ???
0B7F  49 01     EOR #01
0B81  37          ???
0B82  27          ???
0B83  14          ???
0B84  2B          ???
0B85  1D AD 0F  ORA 0FAD,X
0B88  08        PHP 
0B89  43          ???
0B8A  BE 60 0C  LDX 0C60,Y
0B8D  1C          ???
0B8E  2D 3E 51  AND 513E
0B91  3F          ???
0B92  66 7B     ROR 7B
0B94  91 A9     STA (A9),Y
0B96  C3          ???
0B97  DD FA 18  CMP 18FA,X
0B9A  FF          ???
0B9B  38        SEC 
0B9C  5A          ???
0B9D  7D A3 CC  ADC CCA3,X
0BA0  F6 23     INC 23,X
0BA2  53          ???
0BA3  FF          ???
0BA4  86 BB     STX BB
0BA6  F4          ???
0BA7  EB          ???
0BA8  70 B4     BVS 0B5E
0BAA  FB          ???
0BAB  B9 98 ED  LDA ED98,Y
0BAE  47          ???
0BAF  A7          ???
0BB0  0C          ???
0BB1  5F          ???
0BB2  77          ???
0BB3  E9 61     SBC #61
0BB5  E1 68     SBC (68,X)
0BB7  F7          ???
0BB8  FC          ???
0BB9  30 DA     BMI 0B95
0BBB  8F          ???
0BBC  4E AF 18  LSR 18AF
0BBF  D2          ???
0BC0  B5 C3     LDA C3,X
0BC2  D1 EF     CMP (EF),Y
0BC4  17          ???
0BC5  1F          ???
0BC6  60        RTS 
0BC7  B5 1E     LDA 1E,X
0BC9  9C          ???
0BCA  31 FD     AND (FD),Y
0BCC  A5 87     LDA 87
0BCE  86 A2     STX A2
0BD0  AF          ???
0BD1  DF          ???
0BD2  3E C1 6B  ROL 6BC1,X
0BD5  3C          ???
0BD6  39 63 BE  AND BE63,Y
0BD9  FF          ???
0BDA  4B          ???
0BDB  0F          ???
0BDC  0C          ???
0BDD  45 BF     EOR BF
0BDF  7D 83 D6  ADC D683,X
0BE2  FF          ???
0BE3  79 73 C7  ADC C773,Y
0BE6  7C          ???
0BE7  97          ???
0BE8  1E 18 8B  ASL 8B18,X
0BEB  FF          ???
0BEC  7E FA 06  ROR 06FA,X
0BEF  AC F3 E6  LDY E6F3
0BF2  8F          ???
0BF3  F8        SED 
0BF4  FF          ???
0BF5  2E 82 40  ROL 4082
0BF8  01 80     ORA (80,X)
0BFA  02          ???
0BFB  82          ???
0BFC  04          ???
0BFD  03          ???
0BFE  20 04 88  JSR 8804
0C01  25 95     AND 95
0C03  80          ???
0C04  47          ???
0C05  2B          ???
0C06  04          ???
0C07  2E 0A 0B  ROL 0B0A
0C0A  45 89     EOR 89
0C0C  70 40     BVS 0C4E
0C0E  D5 12     CMP 12,X
0C10  0B          ???
0C11  B5 42     LDA 42,X
0C13  15 1C     ORA 1C,X
0C15  1D 1F 21  ORA 211F,X
0C18  CF          ???
0C19  23          ???
0C1A  25 27     AND 27
0C1C  2A        ROL A
0C1D  2C 2F 32  BIT 322F
0C20  35 FF     AND FF,X
0C22  38        SEC 
0C23  3B          ???
0C24  3F          ???
0C25  43          ???
0C26  47          ???
0C27  4B          ???
0C28  4F          ???
0C29  54          ???
0C2A  FF          ???
0C2B  59 5E 64  EOR 645E,Y
0C2E  6A        ROR A
0C2F  70 77     BVS 0CA8
0C31  7E 86 FF  ROR FF86,X
0C34  8E 96 9F  STX 9F96
0C37  A8        TAY 
0C38  B3          ???
0C39  BD C8 D4  LDA D4C8,X
0C3C  FF          ???
0C3D  E1 EE     SBC (EE,X)
0C3F  FD F6 8E  SBC 8EF6,X
0C42  05 0A     ORA 0A
0C44  75 63     ADC 63,X
0C46  13          ???
0C47  0F          ???
0C48  B3          ???
0C49  19 BB 23  ORA 23BB,Y
0C4C  B3          ???
0C4D  48        PHA 
0C4E  58        CLI 
0C4F  E4 04     CPX 04
0C51  48        PHA 
0C52  7B          ???
0C53  78        SEI 
0C54  03          ???
0C55  61 88     ADC (88,X)
0C57  25 77     AND 77
0C59  62          ???
0C5A  D9 84 62  CMP 6284,Y
0C5D  A8        TAY 
0C5E  C8        INY 
0C5F  C4 9E     CPY 9E
0C61  24 4A     BIT 4A
0C63  A4 20     LDY 20
0C65  16 A3     ASL A3,X
0C67  22          ???
0C68  22          ???
0C69  3B          ???
0C6A  A4 12     LDY 12
0C6C  76 4C     ROR 4C,X
0C6E  08        PHP 
0C6F  84 57     STY 57
0C71  67          ???
0C72  BB          ???
0C73  04          ???
0C74  87          ???
0C75  39 05 5A  AND 5A05,Y
0C78  36 82     ROL 82,X
0C7A  88        DEY 
0C7B  8C A2 0E  STY 0EA2
0C7E  61 51     ADC (51,X)
0C80  0C          ???
0C81  15 A6     ORA A6,X
0C83  47          ???
0C84  50 48     BVC 0CCE
0C86  0E 6D 10  ASL 106D
0C89  80          ???
0C8A  13          ???
0C8B  11 09     ORA (09),Y
0C8D  81 AB     STA (AB,X)
0C8F  41 FF     EOR (FF,X)
0C91  C4 52     CPY 52
0C93  05 FA     ORA FA
0C95  A3          ???
0C96  4C B1 AC  JMP ACB1
0C99  29 CA     AND #CA
0C9B  95 0F     STA 0F,X
0C9D  3D 02 CE  AND CE02,X
0CA0  F1 AD     SBC (AD),Y
0CA2  AA        TAX 
0CA3  C2          ???
0CA4  17          ???
0CA5  FA          ???
0CA6  41 03     EOR (03,X)
0CA8  19 7F A0  ORA A07F,Y
0CAB  04          ???
0CAC  1E B9 CF  ASL CFB9,X
0CAF  4B          ???
0CB0  2E 2E B9  ROL B92E
0CB3  24 21     BIT 21
0CB5  20 EB 41  JSR 41EB
0CB8  84 81     STY 81
0CBA  08        PHP 
0CBB  8E 37 82  STX 8237
0CBE  83          ???
0CBF  BF          ???
0CC0  60        RTS 
0CC1  45 5F     EOR 5F
0CC3  02          ???
0CC4  A4 22     LDY 22
0CC6  43          ???
0CC7  73          ???
0CC8  CF          ???
0CC9  05 04     ORA 04
0CCB  24 71     BIT 71
0CCD  32          ???
0CCE  FF          ???
0CCF  05 2C     ORA 2C
0CD1  BE CF 3F  LDX 3FCF,Y
0CD4  EB          ???
0CD5  82          ???
0CD6  15 43     ORA 43,X
0CD8  2E 37 02  ROL 0237
0CDB  2A        ROL A
0CDC  34          ???
0CDD  CC 89 03  CPY 0389
0CE0  DA          ???
0CE1  2B          ???
0CE2  02          ???
0CE3  F8        SED 
0CE4  EA        NOP 
0CE5  04          ???
0CE6  05 EB     ORA EB
0CE8  2A        ROL A
0CE9  1E 94 82  ASL 8294,X
0CEC  08        PHP 
0CED  C4 07     CPY 07
0CEF  E5 08     SBC 08
0CF1  80          ???
0CF2  AA        TAX 
0CF3  15 CA     ORA CA,X
0CF5  06 5D     ASL 5D
0CF7  09 F8     ORA #F8
0CF9  05 6E     ORA 6E
0CFB  0A        ASL A
0CFC  04          ???
0CFD  0B          ???
0CFE  D7          ???
0CFF  C2          ???
0D00  96 0C     STX 0C,Y
0D02  D6 C7     DEC C7,X
0D04  0E 0F F9  ASL F90F
0D07  2A        ROL A
0D08  AC A0 95  LDY 95A0
0D0B  10 11     BPL 0D1E
0D0D  0D 12 F8  ORA F812
0D10  88        DEY 
0D11  42          ???
0D12  14          ???
0D13  13          ???
0D14  15 7E     ORA 7E,X
0D16  99 86 C9  STA C986,Y
0D19  1A          ???
0D1A  19 1B C2  ORA C21B,Y
0D1D  0B          ???
0D1E  7C          ???
0D1F  94 A9     STY A9,X
0D21  C3          ???
0D22  A0 1C     LDY #1C
0D24  F8        SED 
0D25  04          ???
0D26  5F          ???
0D27  55 5B     EOR 5B,X
0D29  76 95     ROR 95,X
0D2B  B3          ???
0D2C  BE CD E8  LDX E8CD,Y
0D2F  FF          ???
0D30  05 0E     ORA 0E
0D32  2A        ROL A
0D33  39 3C 57  AND 573C,Y
0D36  78        SEI 
0D37  97          ???
0D38  FF          ???
0D39  A2 B9     LDX #B9
0D3B  BD C1 D8  LDA D8C1,X
0D3E  DB          ???
0D3F  04          ???
0D40  1A          ???
0D41  FF          ???
0D42  29 36     AND #36
0D44  4F          ???
0D45  5C          ???
0D46  65 F8     ADC F8
0D48  28        PLP 
0D49  16 20     ASL 20,X
0D4B  D0 17     BNE 0D64
0D4D  20 20 2B  JSR 2B20
0D50  EB          ???
0D51  35 21     AND 21,X
0D53  D6 AC     DEC AC,X
0D55  1A          ???
0D56  ED 04 6A  SBC 6A04
0D59  2D 10 54  AND 5410
0D5C  C7          ???
0D5D  48        PHA 
0D5E  7A          ???
0D5F  41 A6     EOR (A6,X)
0D61  74          ???
0D62  46 11     LSR 11
0D64  86 81     STX 81
0D66  9A        TXS 
0D67  8E 45 4B  STX 4B45
0D6A  2C BD 16  BIT 16BD
0D6D  7B          ???
0D6E  49 2E     EOR #2E
0D70  D9 DB 1B  CMP 1BDB,Y
0D73  A5 1D     LDA 1D
0D75  00        BRK 
0D76  7D 63 5A  ADC 5A63,X
0D79  17          ???
0D7A  47          ???
0D7B  55 83     EOR 83,X
0D7D  4B          ???
0D7E  5E 19 A5  LSR A519,X
0D81  16 63     ASL 63,X
0D83  16 17     ASL 17,X
0D85  5B          ???
0D86  8C 88 F9  STY F988
0D89  95 8C     STA 8C,X
0D8B  94 30     STY 30,X
0D8D  19 80 CD  ORA CD80,Y
0D90  DD ED 69  CMP 69ED,X
0D93  29 75     AND #75
0D95  1C          ???
0D96  74          ???
0D97  07          ???
0D98  77          ???
0D99  ED 82 60  SBC 6082
0D9C  16 87     ASL 87,X
0D9E  AF          ???
0D9F  5F          ???
0DA0  F8        SED 
0DA1  CB          ???
0DA2  FA          ???
0DA3  4B          ???
0DA4  FC          ???
0DA5  2B          ???
0DA6  E8        INX 
0DA7  F1 05     SBC (05),Y
0DA9  0C          ???
0DAA  B9 82 2C  LDA 2C82,Y
0DAD  9E          ???
0DAE  31 C2     AND (C2),Y
0DB0  9E          ???
0DB1  33          ???
0DB2  42          ???
0DB3  9E          ???
0DB4  C3          ???
0DB5  86 52     STX 52
0DB7  33          ???
0DB8  45 4D     EOR 4D
0DBA  25 23     AND 23
0DBC  E3          ???
0DBD  25 23     AND 23
0DBF  A3          ???
0DC0  2D 30 AC  AND AC30
0DC3  65 98     ADC 98
0DC5  83          ???
0DC6  1F          ???
0DC7  DB          ???
0DC8  62          ???
0DC9  89          ???
0DCA  7F          ???
0DCB  41 8A     EOR (8A,X)
0DCD  6F          ???
0DCE  3F          ???
0DCF  5F          ???
0DD0  FF          ???
0DD1  3D 00 FF  AND FF00,X
0DD4  63          ???
0DD5  5A          ???
0DD6  4F          ???
0DD7  19 1D D5  ORA D51D,Y
0DDA  83          ???
0DDB  A9 2A     LDA #2A
0DDD  55 2A     EOR 2A,X
0DDF  35 6A     AND 6A,X
0DE1  22          ???
0DE2  D6 81     DEC 81,X
0DE4  37          ???
0DE5  84 AF     STY AF
0DE7  27          ???
0DE8  29 D7     AND #D7
0DEA  70 24     BVS 0E10
0DEC  AF          ???
0DED  E4 C7     CPX C7
0DEF  28        PLP 
0DF0  91 D4     STA (D4),Y
0DF2  CB          ???
0DF3  E4 B0     CPX B0
0DF5  67          ???
0DF6  FA          ???
0DF7  DC          ???
0DF8  7F          ???
0DF9  AC A3 88  LDY 88A3
0DFC  95 A7     STA A7,X
0DFE  31 D9     AND (D9),Y
0E00  14          ???
0E01  B8        CLV 
0E02  AF          ???
0E03  95 0D     STA 0D,X
0E05  B2          ???
0E06  38        SEC 
0E07  1B          ???
0E08  FB          ???
0E09  E6 65     INC 65
0E0B  BE 37 39  LDX 3937,Y
0E0E  43          ???
0E0F  B5 FE     LDA FE,X
0E11  67          ???
0E12  B7          ???
0E13  D6 1A     DEC 1A,X
0E15  25 2D     AND 2D
0E17  59 55 24  EOR 2455,Y
0E1A  88        DEY 
0E1B  60        RTS 
0E1C  49 EE     EOR #EE
0E1E  5F          ???
0E1F  F9 D7 BD  SBC BDD7,Y
0E22  54          ???
0E23  B5 BB     LDA BB,X
0E25  F0 42     BEQ 0E69
0E27  FA          ???
0E28  C1 43     CMP (43,X)
0E2A  87          ???
0E2B  3F          ???
0E2C  2F          ???
0E2D  66 4C     ROR 4C
0E2F  3C          ???
0E30  B1 67     LDA (67),Y
0E32  49 A9     EOR #A9
0E34  02          ???
0E35  1C          ???
0E36  A9 12     LDA #12
0E38  71 01     ADC (01),Y
0E3A  D9 0E 2C  CMP 2C0E,Y
0E3D  46 48     LSR 48
0E3F  CD D8 AC  CMP ACD8
0E42  1D 60 5F  ORA 5F60,X
0E45  BD C8 A8  LDA A8C8,X
0E48  ED A4 6F  SBC 6FA4
0E4B  DF          ???
0E4C  00        BRK 
0E4D  FE 96 CB  INC CB96,X
0E50  85 3B     STA 3B
0E52  48        PHA 
0E53  4D AE FE  EOR FEAE
0E56  16 65     ASL 65,X
0E58  52          ???
0E59  99 A4 19  STA 19A4,Y
0E5C  C4 5A     CPY 5A
0E5E  D5 E3     CMP E3,X
0E60  19 83 3F  ORA 3F83,Y
0E63  3F          ???
0E64  A2 EE     LDX #EE
0E66  BC 00 86  LDY 8600,X
0E69  DC          ???
0E6A  9D 48 12  STA 1248,X
0E6D  46 8B     LSR 8B
0E6F  FC          ???
0E70  63          ???
0E71  55 15     EOR 15,X
0E73  43          ???
0E74  A8        TAY 
0E75  F2          ???
0E76  2C 61 00  BIT 0061
0E79  EF          ???
0E7A  47          ???
0E7B  68        PLA 
0E7C  52          ???
0E7D  A0 A4     LDY #A4
0E7F  61 C9     ADC (C9,X)
0E81  63          ???
0E82  49 28     EOR #28
0E84  C6 B7     DEC B7
0E86  D3          ???
0E87  C4 15     CPY 15
0E89  AA        TAX 
0E8A  36 46     ROL 46,X
0E8C  B4 6D     LDY 6D,X
0E8E  F2          ???
0E8F  0C          ???
0E90  41 B2     EOR (B2,X)
0E92  2E 44 F6  ROL F644
0E95  06 C1     ASL C1
0E97  5A          ???
0E98  43          ???
0E99  A6 5F     LDX 5F
0E9B  F9 31 A4  SBC A431,Y
0E9E  06 AA     ASL AA
0EA0  3D 72 1E  AND 1E72,X
0EA3  AC D8 31  LDY 31D8
0EA6  45 BA     EOR BA
0EA8  BA        TSX 
0EA9  63          ???
0EAA  98        TYA 
0EAB  69 69     ADC #69
0EAD  3C          ???
0EAE  FD C3 BC  SBC BCC3,X
0EB1  71 C2     ADC (C2),Y
0EB3  6B          ???
0EB4  06 4C     ASL 4C
0EB6  DC          ???
0EB7  09 00     ORA #00
0EB9  A0 90     LDY #90
0EBB  6C B3 57  JMP (57B3)
0EBE  25 6A     AND 6A
0EC0  D3          ???
0EC1  37          ???
0EC2  AF          ???
0EC3  B8        CLV 
0EC4  5C          ???
0EC5  89          ???
0EC6  33          ???
0EC7  B5 9B     LDA 9B,X
0EC9  63          ???
0ECA  5A          ???
0ECB  6D 99 B7  ADC B799
0ECE  42          ???
0ECF  65 6D     ADC 6D
0ED1  38        SEC 
0ED2  FC          ???
0ED3  CB          ???
0ED4  B5 67     LDA 67,X
0ED6  3A          ???
0ED7  74          ???
0ED8  6F          ???
0ED9  5F          ???
0EDA  85 69     STA 69
0EDC  DF          ???
0EDD  A6 9B     LDX 9B
0EDF  50 8C     BVC 0E6D
0EE1  14          ???
0EE2  85 35     STA 35
0EE4  52          ???
0EE5  90 26     BCC 0F0D
0EE7  89          ???
0EE8  D1 48     CMP (48),Y
0EEA  91 A1     STA (A1),Y
0EEC  DD 46 8B  CMP 8B46,X
0EEF  63          ???
0EF0  EB          ???
0EF1  33          ???
0EF2  8C 61 41  STY 4161
0EF5  4D FC CC  EOR CCFC
0EF8  0F          ???
0EF9  8E 31 14  STX 1431
0EFC  A0 FF     LDY #FF
0EFE  17          ???
0EFF  CF          ???
0F00  19 35 2C  ORA 2C35,Y
0F03  37          ???
0F04  B2          ???
0F05  24 23     BIT 23
0F07  CB          ???
0F08  8C 2D 3B  STY 3B2D
0F0B  A9 FE     LDA #FE
0F0D  5E 77 59  LSR 5977,X
0F10  36 5A     ROL 5A,X
0F12  38        SEC 
0F13  B8        CLV 
0F14  B5 A4     LDA A4,X
0F16  21 D2     AND (D2,X)
0F18  15 5A     ORA 5A,X
0F1A  22          ???
0F1B  B9 BB 16  LDA 16BB,Y
0F1E  95 20     STA 20,X
0F20  88        DEY 
0F21  AA        TAX 
0F22  FF          ???
0F23  D7          ???
0F24  EA        NOP 
0F25  55 DC     EOR DC,X
0F27  13          ???
0F28  65 3B     ADC 3B
0F2A  A2 17     LDX #17
0F2C  8E 7B B2  STX B27B
0F2F  06 4A     ASL 4A
0F31  79 60 78  ADC 7860,Y
0F34  0B          ???
0F35  77          ???
0F36  A0 71     LDY #71
0F38  01 F8     ORA (F8,X)
0F3A  83          ???
0F3B  4D 21 FB  EOR FB21
0F3E  A2 72     LDX #72
0F40  AF          ???
0F41  55 FD     EOR FD,X
0F43  2C 9E 4C  BIT 4C9E
0F46  30 C6     BMI 0F0E
0F48  BC A5 0C  LDY 0CA5,X
0F4B  B5 A4     LDA A4,X
0F4D  99 37 B5  STA B537,Y
0F50  A4 D5     LDY D5
0F52  B5 3A     LDA 3A,X
0F54  B5 92     LDA 92,X
0F56  AA        TAX 
0F57  2F          ???
0F58  F2          ???
0F59  1E 29 14  ASL 1429,X
0F5C  8E C3 DE  STX DEC3
0F5F  27          ???
0F60  DE 88 8C  DEC 8C88,X
0F63  A6 73     LDX 73
0F65  AD DE 8D  LDA 8DDE
0F68  50 1F     BVC 0F89
0F6A  A7          ???
0F6B  AE 97 BB  LDX BB97
0F6E  A0 50     LDY #50
0F70  67          ???
0F71  BC FF 3F  LDY 3FFF,X
0F74  72          ???
0F75  02          ???
0F76  86 03     STX 03
0F78  B5 FE     LDA FE,X
0F7A  DB          ???
0F7B  0F          ???
0F7C  10 87     BPL 0F05
0F7E  95 8A     STA 8A,X
0F80  82          ???
0F81  18        CLC 
0F82  20 D4 1A  JSR 1AD4
0F85  FD 9B C8  SBC C89B,X
0F88  94 D5     STY D5,X
0F8A  53          ???
0F8B  84 B6     STY B6
0F8D  E1 04     SBC (04,X)
0F8F  AC 19 06  LDY 0619
0F92  A1 57     LDA (57,X)
0F94  60        RTS 
0F95  7D EE 86  ADC 86EE,X
0F98  60        RTS 
0F99  C2          ???
0F9A  7C          ???
0F9B  19 8B F0  ORA F08B,Y
0F9E  A2 2F     LDX #2F
0FA0  BD A4 1A  LDA 1AA4,X
0FA3  9D FF CF  STA CFFF,X
0FA6  FF          ???
0FA7  36 F7     ROL F7,X
0FA9  79 75 47  ADC 4775,Y
0FAC  58        CLI 
0FAD  C7          ???
0FAE  A2 20     LDX #20
0FB0  C0 1E     CPY #1E
0FB2  EA        NOP 
0FB3  48        PHA 
0FB4  54          ???
0FB5  F0 7A     BEQ 1031
0FB7  B6 E3     LDX E3,Y
0FB9  AA        TAX 
0FBA  49 01     EOR #01
0FBC  5F          ???
0FBD  30 AD     BMI 0F6C
0FBF  68        PLA 
0FC0  8D 33 2F  STA 2F33
0FC3  7D B6 02  ADC 02B6,X
0FC6  4C E3 91  JMP 91E3
0FC9  FE 8D 3A  INC 3A8D,X
0FCC  A0 FE     LDY #FE
0FCE  BA        TSX 
0FCF  C1 F3     CMP (F3,X)
0FD1  A2 08     LDX #08
0FD3  B5 89     LDA 89,X
0FD5  C3          ???
0FD6  E9 FD     SBC #FD
0FD8  20 2E A8  JSR A82E
0FDB  FD 2E B0  SBC B02E,X
0FDE  13          ???
0FDF  A2 08     LDX #08
0FE1  D3          ???
0FE2  89          ???
0FE3  E1 E9     SBC (E9,X)
0FE5  FD 40 2E  SBC 2E40,X
0FE8  A8        TAY 
0FE9  FB          ???
0FEA  2E B0 3C  ROL 3CB0
0FED  E8        INX 
0FEE  82          ???
0FEF  F1 22     SBC (22),Y
0FF1  EE FF 19  INC 19FF
0FF4  57          ???
0FF5  AE 60 DD  LDX DD60
0FF8  50 F7     BVC 0FF1
0FFA  5D 1E 61  EOR 611E,X
0FFD  74          ???
0FFE  41 0F     EOR (0F,X)
1000  11 3D     ORA (3D),Y
1002  1D 3F 80  ORA 803F,X
1005  A5 D5     LDA D5
1007  EF          ???
1008  05 D6     ORA D6
100A  28        PLP 
100B  17          ???
100C  44          ???
100D  11 2D     ORA (2D),Y
100F  13          ???
1010  3B          ???
1011  D3          ???
1012  FA          ???
1013  49 D5     EOR #D5
1015  DF          ???
1016  05 D6     ORA D6
1018  32          ???
1019  17          ???
101A  44          ???
101B  11 4B     ORA (4B),Y
101D  13          ???
101E  59 D3 FA  EOR FAD3,Y
1021  C0 5D     CPY #5D
1023  50 BF     BVC 0FE4
1025  5D 3C 61  EOR 613C,X
1028  74          ???
1029  41 69     EOR (69,X)
102B  11 3D     ORA (3D),Y
102D  77          ???
102E  1A          ???
102F  AE E0 09  LDX 09E0
1032  A0 3F     LDY #3F
1034  14          ???
1035  BD E9 1B  LDA 1BE9,X
1038  1C          ???
1039  98        TYA 
103A  50 79     BVC 10B5
103C  46 0A     LSR 0A
103E  AA        TAX 
103F  F5 BF     SBC BF,X
1041  F0 98     BEQ 0FDB
1043  87          ???
1044  42          ???
1045  EE 95 1A  INC 1A95
1048  57          ???
1049  4C 79 38  JMP 3879
104C  2C 70 B2  BIT B270
104F  A8        TAY 
1050  E0 CB     CPX #CB
1052  2E D2 50  ROL 50D2
1055  88        DEY 
1056  5D CB E1  EOR E1CB,X
1059  0B          ???
105A  5C          ???
105B  E5 49     SBC 49
105D  D8        CLD 
105E  5A          ???
105F  18        CLC 
1060  D5 0A     CMP 0A,X
1062  96 27     STX 27,Y
1064  E0 07     CPX #07
1066  01 A0     ORA (A0,X)
1068  06 7A     ASL 7A
106A  FF          ???
106B  82          ???
106C  1A          ???
106D  CC 06 EA  CPY EA06
1070  E0 57     CPX #57
1072  E5 35     SBC 35
1074  7A          ???
1075  FF          ???
1076  06 0B     ASL 0B
1078  08        PHP 
1079  8A        TXA 
107A  29 07     AND #07
107C  09 10     ORA #10
107E  3F          ???
107F  E1 0C     SBC (0C,X)
1081  88        DEY 
1082  98        TYA 
1083  CC 27 3F  CPY 3F27
1086  F9 97 52  SBC 5297,Y
1089  86 4D     STX 4D
108B  E6 2D     INC 2D
108D  90 FB     BCC 108A
108F  0A        ASL A
1090  65 6F     ADC 6F
1092  2E FC A5  ROL A5FC
1095  BA        TSX 
1096  69 B5     ADC #B5
1098  85 FD     STA FD
109A  B8        CLV 
109B  5F          ???
109C  91 46     STA (46),Y
109E  3F          ???
109F  49 1F     EOR #1F
10A1  99 9E A8  STA A89E,Y
10A4  40        RTI 
10A5  3A          ???
10A6  1A          ???
10A7  47          ???
10A8  DF          ???
10A9  B3          ???
10AA  D4          ???
10AB  C3          ???
10AC  88        DEY 
10AD  03          ???
10AE  47          ???
10AF  48        PHA 
10B0  C0 F4     CPY #F4
10B2  A0 33     LDY #33
10B4  91 5B     STA (5B),Y
10B6  09 25     ORA #25
10B8  A5 60     LDA 60
10BA  4A        LSR A
10BB  4A        LSR A
10BC  22          ???
10BD  7D 0F 19  ADC 190F,X
10C0  0A        ASL A
10C1  13          ???
10C2  D6 0B     DEC 0B,X
10C4  98        TYA 
10C5  F3          ???
10C6  AC 04 A8  LDY A804
10C9  73          ???
10CA  E8        INX 
10CB  D0 E3     BNE 10B0
10CD  A2 2B     LDX #2B
10CF  FC          ???
10D0  59 29 7F  EOR 7F29,Y
10D3  09 C7     ORA #C7
10D5  80          ???
10D6  B5 BE     LDA BE,X
10D8  F2          ???
10D9  A0 36     LDY #36
10DB  D2          ???
10DC  BC EE 22  LDY 22EE,X
10DF  8C 28 C6  STY C628
10E2  0D AD 9C  ORA 9CAD
10E5  4C 8E AB  JMP AB8E
10E8  A2 80     LDX #80
10EA  4D AF D1  EOR D1AF
10ED  10 01     BPL 10F0
10EF  C4 84     CPY 84
10F1  08        PHP 
10F2  FC          ???
10F3  9A        TXS 
10F4  4A        LSR A
10F5  89          ???
10F6  2D CA 2D  AND 2DCA
10F9  EB          ???
10FA  79 7D 8F  ADC 8F7D,Y
10FD  00        BRK 
10FE  0D 8D 9B  ORA 9B8D
1101  A9 12     LDA #12
1103  9C          ???
1104  86 30     STX 30
1106  60        RTS 
1107  A9 37     LDA #37
1109  BF          ???
110A  ED 6E 56  SBC 566E
110D  4A        LSR A
110E  30 12     BMI 1122
1110  8C 74 B9  STY B974
1113  E4 9A     CPX 9A
1115  04          ???
1116  CB          ???
1117  2C 10 F3  BIT F310
111A  4C 69 3E  JMP 3E69
111D  E9 C7     SBC #C7
111F  8D B7 A8  STA A8B7
1122  1B          ???
1123  A0 0E     LDY #0E
1125  F6 D3     INC D3,X
1127  97          ???
1128  FE 1C 99  INC 991C,X
112B  71 97     ADC (97),Y
112D  88        DEY 
112E  0A        ASL A
112F  53          ???
1130  F1 B2     SBC (B2),Y
1132  09 F9     ORA #F9
1134  C4 9D     CPY 9D
1136  F8        SED 
1137  AC E8 E0  LDY E0E8
113A  07          ???
113B  AE F5 AD  LDX ADF5
113E  D6 D2     DEC D2,X
1140  6A        ROR A
1141  0C          ???
1142  D5 B3     CMP B3,X
1144  86 2C     STX 2C
1146  31 9D     AND (9D),Y
1148  2C C5 D0  BIT D0C5
114B  EF          ???
114C  9A        TXS 
114D  81 07     STA (07,X)
114F  71 0F     ADC (0F),Y
1151  0D 8D B9  ORA B98D
1154  FA          ???
1155  41 29     EOR (29,X)
1157  3F          ???
1158  9D 1B 5A  STA 5A1B,X
115B  24 B5     BIT B5
115D  30 90     BMI 10EF
115F  6F          ???
1160  93          ???
1161  3A          ???
1162  B0 05     BCS 1169
1164  38        SEC 
1165  E9 13     SBC #13
1167  D0 7F     BNE 11E8
1169  17          ???
116A  2E D5 53  ROL 53D5
116D  7F          ???
116E  C2          ???
116F  6A        ROR A
1170  0A        ASL A
1171  C9 B0     CMP #B0
1173  27          ???
1174  09 9A     ORA #9A
1176  B7          ???
1177  91 07     STA (07),Y
1179  44          ???
117A  1C          ???
117B  F8        SED 
117C  7C          ???
117D  27          ???
117E  18        CLC 
117F  69 94     ADC #94
1181  F2          ???
1182  1F          ???
1183  07          ???
1184  96 E6     STX E6,Y
1186  AD D0 03  LDA 03D0
1189  EE E7 1B  INC 1BE7
118C  AC 7E 92  LDY 927E
118F  13          ???
1190  64          ???
1191  8F          ???
1192  E4 4D     CPX 4D
1194  71 93     ADC (93),Y
1196  91 36     STA (36),Y
1198  53          ???
1199  4E 44 A7  LSR A744
119C  35 5C     AND 5C,X
119E  89          ???
119F  B2          ???
11A0  17          ???
11A1  1A          ???
11A2  7D 26 F9  ADC F926,X
11A5  C9 28     CMP #28
11A7  B6 8D     LDX 8D,Y
11A9  DB          ???
11AA  4C AA BD  JMP BDAA
11AD  19 D8 E1  ORA E1D8,Y
11B0  AD 0A 6E  LDA 6E0A
11B3  19 02 A0  ORA A002,Y
11B6  C1 8C     CMP (8C,X)
11B8  AF          ???
11B9  10 D0     BPL 118B
11BB  60        RTS 
11BC  F1 38     SBC (38),Y
11BE  70 A8     BVS 1168
11C0  E0 18     CPX #18
11C2  50 BF     BVC 1183
11C4  88        DEY 
11C5  F9 B5 09  SBC 09B5,Y
11C8  91 18     STA (18),Y
11CA  11 20     ORA (20),Y
11CC  87          ???
11CD  D4          ???
11CE  51 85     EOR (85),Y
11D0  88        DEY 
11D1  0A        ASL A
11D2  97          ???
11D3  AB          ???
11D4  16 54     ASL 54,X
11D6  21 D2     AND (D2,X)
11D8  8C 08 0A  STY 0A08
11DB  39 44 44  AND 4444,Y
11DE  F8        SED 
11DF  20 88 58  JSR 5888
11E2  02          ???
11E3  C2          ???
11E4  A4 F7     LDY F7
11E6  35 9F     AND 9F,X
11E8  16 10     ASL 10,X
11EA  A7          ???
11EB  96 A1     STX A1,Y
11ED  A8        TAY 
11EE  41 47     EOR (47,X)
11F0  28        PLP 
11F1  07          ???
11F2  01 44     ORA (44,X)
11F4  01 60     ORA (60,X)
11F6  94 84     STY 84,X
11F8  8F          ???
11F9  25 8A     AND 8A
11FB  94 A1     STY A1,X
11FD  9A        TXS 
11FE  40        RTI 
11FF  D0 0E     BNE 120F
1201  9F          ???
1202  AF          ???
1203  B0 50     BCS 1255
1205  98        TYA 
1206  61 A3     ADC (A3,X)
1208  D1 02     CMP (02),Y
120A  E8        INX 
120B  8B          ???
120C  02          ???
120D  08        PHP 
120E  49 4C     EOR #4C
1210  2C 40 A2  BIT A240
1213  11 02     ORA (02),Y
1215  82          ???
1216  01 8B     ORA (8B,X)
1218  68        PLA 
1219  00        BRK 
121A  46 01     LSR 01
121C  54          ???
121D  D8        CLD 
121E  13          ???
121F  78        SEI 
1220  04          ???
1221  80          ???
1222  88        DEY 
1223  0C          ???
1224  E0 88     CPX #88
1226  0F          ???
1227  28        PLP 
1228  3A          ???
1229  01 2C     ORA (2C,X)
122B  C0 A0     CPY #A0
122D  0A        ASL A
122E  06 07     ASL 07
1230  F2          ???
1231  E8        INX 
1232  F0 3D     BEQ 1271
1234  33          ???
1235  7E F5 AD  ROR ADF5,X
1238  92          ???
1239  1E A9 AA  ASL AAA9,X
123C  00        BRK 
123D  6A        ROR A
123E  A6 9A     LDX 9A
1240  D9 2A 46  CMP 462A,Y
1243  9A        TXS 
1244  28        PLP 
1245  A6 CE     LDX CE
1247  A9 AD     LDA #AD
1249  92          ???
124A  6E A9 AA  ROR AAA9
124D  50 6B     BVC 12BA
124F  F6 75     INC 75,X
1251  1C          ???
1252  E4 04     CPX 04
1254  AE 96 9D  LDX 9D96
1257  F5 11     SBC 11,X
1259  78        SEI 
125A  C1 7A     CMP (7A,X)
125C  79 26 2E  ADC 2E26,Y
125F  F0 15     BEQ 1276
1261  A0 07     LDY #07
1263  BD 9F C5  LDA C59F,X
1266  1F          ???
1267  DD F9 40  CMP 40F9,X
126A  EE FA D3  INC D3FA
126D  FD EB FE  SBC FEEB,X
1270  D6 7A     DEC 7A,X
1272  88        DEY 
1273  E6 ED     INC ED
1275  E0 2B     CPX #2B
1277  AE 50 E6  LDX E650
127A  E6 FE     INC FE
127C  1B          ???
127D  25 A6     AND A6
127F  96 BE     STX BE,Y
1281  1D 9D 51  ORA 519D,X
1284  6C 85 04  JMP (0485)
1287  96 AA     STX AA,Y
1289  E7          ???
128A  E3          ???
128B  CC 1C 40  CPY 401C
128E  9C          ???
128F  CE 9C 48  DEC 489C
1292  9C          ???
1293  D0 9C     BNE 1231
1295  50 9C     BVC 1233
1297  D2          ???
1298  9C          ???
1299  58        CLI 
129A  9C          ???
129B  D4          ???
129C  9C          ???
129D  60        RTS 
129E  9C          ???
129F  D8        CLD 
12A0  9C          ???
12A1  68        PLA 
12A2  9C          ???
12A3  DA          ???
12A4  9C          ???
12A5  70 9C     BVS 1243
12A7  DC          ???
12A8  9C          ???
12A9  78        SEI 
12AA  9C          ???
12AB  DE 9C 80  DEC 809C,X
12AE  9C          ???
12AF  E0 9C     CPX #9C
12B1  88        DEY 
12B2  9C          ???
12B3  E2          ???
12B4  9C          ???
12B5  90 9C     BCC 1253
12B7  E4 9C     CPX 9C
12B9  98        TYA 
12BA  9C          ???
12BB  E6 9C     INC 9C
12BD  87          ???
12BE  B2          ???
12BF  16 A8     ASL A8,X
12C1  C9 BD     CMP #BD
12C3  EA        NOP 
12C4  EB          ???
12C5  2F          ???
12C6  99 B0 FF  STA FFB0,Y
12C9  51 F8     EOR (F8),Y
12CB  C0 09     CPY #09
12CD  C9 1E     CMP #1E
12CF  EE 62 3E  INC 3E62
12D2  0A        ASL A
12D3  0E 05 DF  ASL DF05
12D6  2B          ???
12D7  1E A3 A0  ASL A0A3,X
12DA  7F          ???
12DB  B9 40 03  LDA 0340,Y
12DE  91 FD     STA (FD),Y
12E0  88        DEY 
12E1  F7          ???
12E2  F8        SED 
12E3  35 F7     AND F7,X
12E5  AA        TAX 
12E6  BE 2D 21  LDX 212D,Y
12E9  DC          ???
12EA  29 B6     AND #B6
12EC  83          ???
12ED  32          ???
12EE  51 5C     EOR (5C),Y
12F0  10 C4     BPL 12B6
12F2  F0 04     BEQ 12F8
12F4  CE CF 43  DEC 43CF
12F7  60        RTS 
12F8  3C          ???
12F9  72          ???
12FA  A1 F1     LDA (F1,X)
12FC  26 C9     ROL C9
12FE  BD 88 EB  LDA EB88,X
1301  1F          ???
1302  86 25     STX 25
1304  94 9A     STY 9A,X
1306  61 C5     ADC (C5,X)
1308  47          ???
1309  13          ???
130A  30 04     BMI 1310
130C  C9 80     CMP #80
130E  4F          ???
130F  90 03     BCC 1314
1311  D9 41 F1  CMP F141,Y
1314  38        SEC 
1315  9F          ???
1316  B2          ???
1317  89          ???
1318  EE BA 1E  INC 1EBA
131B  60        RTS 
131C  78        SEI 
131D  5F          ???
131E  78        SEI 
131F  11 63     ORA (63),Y
1321  17          ???
1322  13          ???
1323  1C          ???
1324  12          ???
1325  52          ???
1326  1D ED 7F  ORA 7FED,X
1329  69 88     ADC #88
132B  8E C2 DC  STX DCC2
132E  1F          ???
132F  B7          ???
1330  78        SEI 
1331  8D 1A CA  STA CA1A
1334  CE 19 D0  DEC D019
1337  F1 18     SBC (18),Y
1339  A7          ???
133A  D4          ???
133B  A6 6A     LDX 6A
133D  1F          ???
133E  0A        ASL A
133F  96 F0     STX F0,Y
1341  31 D5     AND (D5),Y
1343  14          ???
1344  AB          ???
1345  A2 EA     LDX #EA
1347  8E 15 03  STX 0315
134A  5F          ???
134B  DC          ???
134C  07          ???
134D  0F          ???
134E  95 C7     STA C7,X
1350  57          ???
1351  20 6C 04  JSR 046C
1354  4C 05 4C  JMP 4C05
1357  50 AE     BVC 1307
1359  06 A4     ASL A4
135B  86 01     STX 01
135D  2F          ???
135E  C4 09     CPY 09
1360  DA          ???
1361  E8        INX 
1362  7D DC 5A  ADC 5ADC,X
1365  C8        INY 
1366  F0 2F     BEQ 1397
1368  D8        CLD 
1369  23          ???
136A  CD E3 52  CMP 52E3
136D  5F          ???
136E  04          ???
136F  8D A5 3F  STA 3FA5
1372  AD 33 54  LDA 5433
1375  CD 7E FE  CMP FE7E
1378  1A          ???
1379  08        PHP 
137A  B5 FD     LDA FD,X
137C  55 35     EOR 35,X
137E  E8        INX 
137F  A2 12     LDX #12
1381  BD 76 1F  LDA 1F76,X
1384  9D 7F 94  STA 947F,X
1387  75 57     ADC 57,X
1389  C7          ???
138A  6F          ???
138B  15 7C     ORA 7C,X
138D  6E C0 02  ROR 02C0
1390  FE E5 B1  INC B1E5,X
1393  FC          ???
1394  C8        INY 
1395  FD F9 44  SBC 44F9,X
1398  B5 F8     LDA F8,X
139A  F3          ???
139B  C6 01     DEC 01
139D  4C 7B 33  JMP 337B
13A0  0F          ???
13A1  5D 34 55  EOR 5534,X
13A4  8A        TXA 
13A5  08        PHP 
13A6  02          ???
13A7  2F          ???
13A8  54          ???
13A9  8D 05 97  STA 9705
13AC  2A        ROL A
13AD  8E 25 0A  STX 0A25
13B0  59 FF 11  EOR 11FF,Y
13B3  D3          ???
13B4  34          ???
13B5  11 42     ORA (42),Y
13B7  02          ???
13B8  EE EA A9  INC A9EA
13BB  05 0C     ORA 0C
13BD  79 40 21  ADC 2140,Y
13C0  2A        ROL A
13C1  AC 9C 0B  LDY 0B9C
13C4  C6 09     DEC 09
13C6  08        PHP 
13C7  CA        DEX 
13C8  01 0F     ORA (0F,X)
13CA  0E 17 76  ASL 7617
13CD  82          ???
13CE  95 7B     STA 7B,X
13D0  28        PLP 
13D1  01 29     ORA (29,X)
13D3  8D 31 39  STA 3931
13D6  6B          ???
13D7  38        SEC 
13D8  37          ???
13D9  CB          ???
13DA  43          ???
13DB  49 4E     EOR #4E
13DD  7E 4D B2  ROR B24D,X
13E0  57          ???
13E1  41 C1     EOR (C1,X)
13E3  D5 E0     CMP E0,X
13E5  22          ???
13E6  B8        CLV 
13E7  E9 FA     SBC #FA
13E9  18        CLC 
13EA  9D 4F 76  STA 764F,X
13ED  5E 27 2F  LSR 2F27,X
13F0  0F          ???
13F1  0F          ???
13F2  0B          ???
13F3  A3          ???
13F4  A2 F1     LDX #F1
13F6  79 38 40  ADC 4038,Y
13F9  5D 39 57  EOR 5739,X
13FC  C2          ???
13FD  D8        CLD 
13FE  64          ???
13FF  78        SEI 
1400  EC 56 BA  CPX BA56
1403  68        PLA 
1404  13          ???
1405  94 5C     STY 5C,X
1407  0B          ???
1408  2E 90 F4  ROL F490
140B  03          ???
140C  B9 FE BD  LDA BDFE,Y
140F  D8        CLD 
1410  61 40     ADC (40,X)
1412  0C          ???
1413  B4 A0     LDY A0,X
1415  E4 3A     CPX 3A
1417  33          ???
1418  0A        ASL A
1419  19 43 27  ORA 2743,Y
141C  D8        CLD 
141D  E3          ???
141E  B8        CLV 
141F  D9 D0 FE  CMP FED0,Y
1422  10 13     BPL 1437
1424  04          ???
1425  C1 02     CMP (02,X)
1427  5C          ???
1428  01 06     ORA (06,X)
142A  1C          ???
142B  34          ???
142C  BA        TSX 
142D  A9 35     LDA #35
142F  18        CLC 
1430  51 66     EOR (66),Y
1432  9A        TXS 
1433  0D 60 BC  ORA BC60
1436  3A          ???
1437  41 14     EOR (14,X)
1439  AA        TAX 
143A  56 64     LSR 64,X
143C  77          ???
143D  DD B8 EC  CMP ECB8,X
1440  3C          ???
1441  1D 54 84  ORA 8454,X
1444  31 68     AND (68),Y
1446  FB          ???
1447  FE 06 3E  INC 3E06,X
144A  20 C5 7C  JSR 7CC5
144D  73          ???
144E  08        PHP 
144F  5C          ???
1450  74          ???
1451  C1 E0     CMP (E0,X)
1453  7C          ???
1454  07          ???
1455  0B          ???
1456  1E 2A CF  ASL CF2A,X
1459  2D 9E 6E  AND 6E9E
145C  F8        SED 
145D  E0 EB     CPX #EB
145F  15 BB     ORA BB,X
1461  EF          ???
1462  0D B1 41  ORA 41B1
1465  10 35     BPL 149C
1467  A1 AE     LDA (AE,X)
1469  1A          ???
146A  46 CF     LSR CF
146C  3D 41 F2  AND F241,X
146F  0E D0 A0  ASL A0D0
1472  8D A8 78  STA 78A8
1475  F9 EE AF  SBC AFEE,Y
1478  E7          ???
1479  A8        TAY 
147A  A4 E4     LDY E4
147C  74          ???
147D  5C          ???
147E  F4          ???
147F  7E FC 9C  ROR 9CFC,X
1482  58        CLI 
1483  BC A0 8E  LDY 8EA0,X
1486  CB          ???
1487  07          ???
1488  06 06     ASL 06
148A  FE 1B 1E  INC 1E1B,X
148D  FD FF CF  SBC CFFF,X
1490  EE DB 8A  INC 8ADB
1493  E0 3F     CPX #3F
1495  01 23     ORA (23,X)
1497  86 D0     STX D0
1499  41 90     EOR (90,X)
149B  87          ???
149C  2E 20 05  ROL 0520
149F  74          ???
14A0  7D 14 1B  ADC 1B14,X
14A3  B2          ???
14A4  E5 A4     SBC A4
14A6  74          ???
14A7  28        PLP 
14A8  25 6F     AND 6F
14AA  19 06 49  ORA 4906,Y
14AD  F2          ???
14AE  2E 26 B9  ROL B926
14B1  E0 09     CPX #09
14B3  B4 A6     LDY A6,X
14B5  B8        CLV 
14B6  F8        SED 
14B7  4D CB 20  EOR 20CB
14BA  59 50 1E  EOR 1E50,Y
14BD  E4 41     CPX 41
14BF  50 38     BVC 14F9
14C1  90 4C     BCC 150F
14C3  A0 64     LDY #64
14C5  BD A7 B8  LDA B8A7,X
14C8  32          ???
14C9  C7          ???
14CA  66 0B     ROR 0B
14CC  5C          ???
14CD  7C          ???
14CE  D8        CLD 
14CF  FC          ???
14D0  98        TYA 
14D1  9F          ???
14D2  3E 2A 0E  ROL 0E2A,X
14D5  F5 65     SBC 65,X
14D7  64          ???
14D8  9C          ???
14D9  9C          ???
14DA  6A        ROR A
14DB  01 28     ORA (28,X)
14DD  66 5A     ROR 5A
14DF  78        SEI 
14E0  EC 51 56  CPX 5651
14E3  65 E5     ADC E5
14E5  E4 B9     CPX B9
14E7  1A          ???
14E8  4E 04 C0  LSR C004
14EB  20 D4 C4  JSR C4D4
14EE  0A        ASL A
14EF  0C          ???
14F0  21 E5     AND (E5,X)
14F2  0F          ???
14F3  0D CD A9  ORA A9CD
14F6  C6 35     DEC 35
14F8  C5 81     CMP 81
14FA  C1 97     CMP (97,X)
14FC  86 1C     STX 1C
14FE  D6 03     DEC 03,X
1500  17          ???
1501  80          ???
1502  0F          ???
1503  98        TYA 
1504  BC FC B8  LDY B8FC,X
1507  3D 7B 00  AND 007B,X
150A  EA        NOP 
150B  E0 6B     CPX #6B
150D  64          ???
150E  A4 79     LDY 79
1510  94 6C     STY 6C,X
1512  9C          ???
1513  5C          ???
1514  7C          ???
1515  50 4C     BVC 1563
1517  D0 00     BNE 1519
1519  72          ???
151A  29 42     AND #42
151C  C9 B4     CMP #B4
151E  F0 11     BEQ 1531
1520  E7          ???
1521  56 59     LSR 59,X
1523  CA        DEX 
1524  FB          ???
1525  94 1C     STY 1C,X
1527  56 98     LSR 98,X
1529  97          ???
152A  D5 E8     CMP E8,X
152C  B8        CLV 
152D  4E 42 A4  LSR A442
1530  52          ???
1531  5C          ???
1532  65 B1     ADC B1
1534  AC 09 78  LDY 7809
1537  68        PLA 
1538  E4 3E     CPX 3E
153A  C0 86     CPY #86
153C  83          ???
153D  39 29 2C  AND 2C29,Y
1540  37          ???
1541  7C          ???
1542  64          ???
1543  E0 D8     CPX #D8
1545  03          ???
1546  02          ???
1547  9B          ???
1548  93          ???
1549  61 7D     ADC (7D,X)
154B  14          ???
154C  F0 B3     BEQ 1501
154E  D0 78     BNE 15C8
1550  58        CLI 
1551  FD 9C 16  SBC 169C,X
1554  2E B8 62  ROL 62B8
1557  F4          ???
1558  C1 F6     CMP (F6,X)
155A  B8        CLV 
155B  6C C8 01  JMP (01C8)
155E  11 98     ORA (98),Y
1560  E5 46     SBC 46
1562  81 43     STA (43,X)
1564  52          ???
1565  91 E3     STA (E3),Y
1567  A4 68     LDY 68
1569  2C E0 78  BIT 78E0
156C  A0 B2     LDY #B2
156E  B0 38     BCS 15A8
1570  24 B0     BIT B0
1572  ED 60 59  SBC 5960
1575  F0 2A     BEQ 15A1
1577  1B          ???
1578  30 60     BMI 15DA
157A  A3          ???
157B  87          ???
157C  2C EC F4  BIT F4EC
157F  F4          ???
1580  E0 63     CPX #63
1582  D0 85     BNE 1509
1584  31 18     AND (18),Y
1586  C7          ???
1587  0C          ???
1588  D2          ???
1589  B8        CLV 
158A  EE BA A4  INC A4BA
158D  90 7D     BCC 160C
158F  3B          ???
1590  91 98     STA (98),Y
1592  4C E5 BB  JMP BBE5
1595  17          ???
1596  11 A4     ORA (A4),Y
1598  A2 43     LDX #43
159A  96 33     STX 33,Y
159C  30 64     BMI 1602
159E  39 94 93  AND 9394,Y
15A1  39 8E 54  AND 548E,Y
15A4  E5 2D     SBC 2D
15A6  10 C8     BPL 1570
15A8  75 70     ADC 70,X
15AA  21 0A     AND (0A,X)
15AC  24 FD     BIT FD
15AE  81 28     STA (28,X)
15B0  24 22     BIT 22
15B2  CA        DEX 
15B3  C0 62     CPY #62
15B5  F0 01     BEQ 15B8
15B7  F3          ???
15B8  04          ???
15B9  1B          ???
15BA  19 D8 03  ORA 03D8,Y
15BD  DF          ???
15BE  02          ???
15BF  EC 5E F4  CPX F45E
15C2  7C          ???
15C3  A7          ???
15C4  1C          ???
15C5  7B          ???
15C6  09 DA     ORA #DA
15C8  68        PLA 
15C9  6D 2B 1F  ADC 1F2B
15CC  79 56 BB  ADC BB56,Y
15CF  EA        NOP 
15D0  62          ???
15D1  88        DEY 
15D2  5B          ???
15D3  DC          ???
15D4  74          ???
15D5  5B          ???
15D6  E4 BC     CPX BC
15D8  4B          ???
15D9  24 ED     BIT ED
15DB  84 58     STY 58
15DD  82          ???
15DE  35 F4     AND F4,X
15E0  32          ???
15E1  E7          ???
15E2  44          ???
15E3  B4 A4     LDY A4,X
15E5  B9 54 18  LDA 1854,Y
15E8  77          ???
15E9  57          ???
15EA  02          ???
15EB  03          ???
15EC  0F          ???
15ED  74          ???
15EE  0A        ASL A
15EF  FE 1A E0  INC E01A,X
15F2  90 17     BCC 160B
15F4  C1 80     CMP (80,X)
15F6  41 C5     EOR (C5,X)
15F8  05 16     ORA 16
15FA  86 65     STX 65
15FC  59 4A 8E  EOR 8E4A,Y
15FF  6B          ???
1600  AE 1F 7F  LDX 7F1F
1603  1B          ???
1604  0E FD 40  ASL 40FD
1607  42          ???
1608  A2 A4     LDX #A4
160A  60        RTS 
160B  80          ???
160C  90 50     BCC 165E
160E  5E 39 B4  LSR B439,X
1611  AA        TAX 
1612  C3          ???
1613  E9 AE     SBC #AE
1615  97          ???
1616  54          ???
1617  6C 58 64  JMP (6458)
161A  98        TYA 
161B  19 1E 0B  ORA 0B1E,Y
161E  BF          ???
161F  06 07     ASL 07
1621  02          ???
1622  E2          ???
1623  24 7D     BIT 7D
1625  E1 72     SBC (72,X)
1627  86 D0     STX D0
1629  64          ???
162A  1C          ???
162B  84 B8     STY B8
162D  10 4C     BPL 167B
162F  60        RTS 
1630  B2          ???
1631  06 C1     ASL C1
1633  16 68     ASL 68,X
1635  EF          ???
1636  BE 31 67  LDX 6731,Y
1639  01 AC     ORA (AC,X)
163B  90 B1     BCC 15EE
163D  00        BRK 
163E  50 A2     BVC 15E2
1640  16 C1     ASL C1,X
1642  4D 64 15  EOR 1564
1645  19 05 9F  ORA 9F05,Y
1648  5A          ???
1649  1A          ???
164A  06 65     ASL 65
164C  23          ???
164D  11 44     ORA (44),Y
164F  BB          ???
1650  77          ???
1651  02          ???
1652  70 A0     BVS 15F4
1654  C9 41     CMP #41
1656  94 04     STY 04,X
1658  59 C6 10  EOR 10C6,Y
165B  88        DEY 
165C  0C          ???
165D  3E 54 08  ROL 0854,X
1660  0B          ???
1661  4C 0E 0A  JMP 0A0E
1664  09 27     ORA #27
1666  41 97     EOR (97,X)
1668  01 45     ORA (45,X)
166A  02          ???
166B  AF          ???
166C  1A          ???
166D  48        PHA 
166E  70 10     BVS 1680
1670  40        RTI 
1671  80          ???
1672  C3          ???
1673  17          ???
1674  80          ???
1675  0F          ???
1676  98        TYA 
1677  56 19     LSR 19,X
1679  32          ???
167A  88        DEY 
167B  96 65     STX 65,Y
167D  BB          ???
167E  AF          ???
167F  78        SEI 
1680  32          ???
1681  51 44     EOR (44),Y
1683  EC B8 28  CPX 28B8
1686  1F          ???
1687  18        CLC 
1688  24 58     BIT 58
168A  97          ???
168B  2A        ROL A
168C  ED 1E 09  SBC 091E
168F  06 ED     ASL ED
1691  59 07 01  EOR 0107,Y
1694  18        CLC 
1695  40        RTI 
1696  90 54     BCC 16EC
1698  11 BF     ORA (BF),Y
169A  BA        TSX 
169B  EE 76 08  INC 0876
169E  07          ???
169F  45 68     EOR 68
16A1  DE 20 14  DEC 1420,X
16A4  C2          ???
16A5  27          ???
16A6  55 44     EOR 44,X
16A8  32          ???
16A9  F8        SED 
16AA  E8        INX 
16AB  B4 CB     LDY CB,X
16AD  E4 A9     CPX A9
16AF  E6 F0     INC F0
16B1  9D A5 04  STA 04A5,X
16B4  BA        TSX 
16B5  64          ???
16B6  2D 1B 39  AND 391B
16B9  6C 78 A4  JMP (A478)
16BC  94 65     STY 65,X
16BE  99 FE 51  STA 51FE,Y
16C1  11 09     ORA (09),Y
16C3  10 44     BPL 1709
16C5  58        CLI 
16C6  A4 16     LDY 16
16C8  45 38     EOR 38
16CA  21 74     AND (74,X)
16CC  3A          ???
16CD  8D 64 15  STA 1564
16D0  95 90     STA 90,X
16D2  94 E5     STY E5,X
16D4  60        RTS 
16D5  54          ???
16D6  44          ???
16D7  E5 14     SBC 14
16D9  80          ???
16DA  70 00     BVS 16DC
16DC  63          ???
16DD  A0 1E     LDY #1E
16DF  B6 D4     LDX D4,Y
16E1  02          ???
16E2  42          ???
16E3  F8        SED 
16E4  03          ???
16E5  EB          ???
16E6  C0 03     CPY #03
16E8  1E 0C CA  ASL CA0C,X
16EB  FC          ???
16EC  70 08     BVS 16F6
16EE  6D E0 0E  ADC 0EE0
16F1  AA        TAX 
16F2  FC          ???
16F3  02          ???
16F4  19 0F A0  ORA A00F,Y
16F7  06 2E     ASL 2E
16F9  00        BRK 
16FA  83          ???
16FB  8D 40 FA  STA FA40
16FE  43          ???
16FF  18        CLC 
1700  0F          ???
1701  BC 01 88  LDY 8801,X
1704  80          ???
1705  81 0F     STA (0F,X)
1707  5C          ???
1708  61 85     ADC (85,X)
170A  02          ???
170B  A6 9C     LDX 9C
170D  61 C0     ADC (C0,X)
170F  0E FA 62  ASL 62FA
1712  1C          ???
1713  A8        TAY 
1714  FC          ???
1715  10 C7     BPL 16DE
1717  C0 07     CPY #07
1719  BC 00 FB  LDY FB00,X
171C  C1 F8     CMP (F8,X)
171E  31 50     AND (50),Y
1720  0F          ???
1721  A9 AB     LDA #AB
1723  0C          ???
1724  15 16     ORA 16,X
1726  C0 F6     CPY #F6
1728  81 A4     STA (A4,X)
172A  F2          ???
172B  10 1A     BPL 1747
172D  2F          ???
172E  00        BRK 
172F  D3          ???
1730  A8        TAY 
1731  00        BRK 
1732  7F          ???
1733  66 0A     ROR 0A
1735  A9 03     LDA #03
1737  84 C9     STY C9
1739  39 C7 A6  AND A6C7,Y
173C  D7          ???
173D  22          ???
173E  18        CLC 
173F  6A        ROR A
1740  04          ???
1741  45 B3     EOR B3
1743  7F          ???
1744  10 C7     BPL 170D
1746  80          ???
1747  41 A4     EOR (A4,X)
1749  CC 60 18  CPY 1860
174C  6E D7 02  ROR 02D7
174F  1D 6A 0A  ORA 0A6A,X
1752  01 82     ORA (82,X)
1754  C0 6A     CPY #6A
1756  D0 F5     BNE 174D
1758  FC          ???
1759  E5 9B     SBC 9B
175B  80          ???
175C  2A        ROL A
175D  56 BE     LSR BE,X
175F  25 5E     AND 5E
1761  1F          ???
1762  64          ???
1763  41 8B     EOR (8B,X)
1765  01 50     ORA (50,X)
1767  EE 66 D0  INC D066
176A  01 72     ORA (72,X)
176C  22          ???
176D  78        SEI 
176E  47          ???
176F  5C          ???
1770  81 F5     STA (F5,X)
1772  3E 05 DF  ROL DF05,X
1775  9C          ???
1776  E7          ???
1777  EA        NOP 
1778  86 DC     STX DC
177A  C8        INY 
177B  57          ???
177C  0F          ???
177D  38        SEC 
177E  76 76     ROR 76,X
1780  03          ???
1781  A8        TAY 
1782  A9 19     LDA #19
1784  1E 10 65  ASL 6510,X
1787  A0 88     LDY #88
1789  09 D7     ORA #D7
178B  B1 C6     LDA (C6),Y
178D  97          ???
178E  D7          ???
178F  71 09     ADC (09),Y
1791  5E 07 DC  LSR DC07,X
1794  2F          ???
1795  44          ???
1796  5B          ???
1797  D8        CLD 
1798  88        DEY 
1799  B7          ???
179A  B0 1F     BCS 17BB
179C  5C          ???
179D  2A        ROL A
179E  40        RTI 
179F  0E 24 25  ASL 2524
17A2  06 04     ASL 04
17A4  02          ???
17A5  A5 EA     LDA EA
17A7  8F          ???
17A8  E8        INX 
17A9  20 19 6F  JSR 6F19
17AC  00        BRK 
17AD  F3          ???
17AE  60        RTS 
17AF  2A        ROL A
17B0  42          ???
17B1  A4 F7     LDY F7
17B3  67          ???
17B4  84 4B     STY 4B
17B6  47          ???
17B7  04          ???
17B8  CC 40 D5  CPY D540
17BB  C4 F7     CPY F7
17BD  C4 CC     CPY CC
17BF  DC          ???
17C0  F3          ???
17C1  1A          ???
17C2  0D D1 A0  ORA A0D1
17C5  32          ???
17C6  88        DEY 
17C7  47          ???
17C8  7F          ???
17C9  3B          ???
17CA  25 F0     AND F0
17CC  A9 62     LDA #62
17CE  60        RTS 
17CF  71 B0     ADC (B0),Y
17D1  44          ???
17D2  5C          ???
17D3  63          ???
17D4  0A        ASL A
17D5  A8        TAY 
17D6  18        CLC 
17D7  8C D5 31  STY 31D5
17DA  F5 86     SBC 86,X
17DC  30 CC     BMI 17AA
17DE  AC 1E D7  LDY D71E
17E1  32          ???
17E2  19 54 80  ORA 8054,Y
17E5  08        PHP 
17E6  63          ???
17E7  95 71     STA 71,X
17E9  FD 86 DF  SBC DF86,X
17EC  01 12     ORA (12,X)
17EE  10 D8     BPL 17C8
17F0  EA        NOP 
17F1  97          ???
17F2  30 DB     BMI 17CF
17F4  20 03 2E  JSR 2E03
17F7  02          ???
17F8  A2 24     LDX #24
17FA  C2          ???
17FB  0C          ???
17FC  4A        LSR A
17FD  0B          ???
17FE  FF          ???
17FF  C1 FC     CMP (FC,X)
1801  DF          ???
1802  9A        TXS 
1803  1B          ???
1804  A3          ???
1805  40        RTI 
1806  8F          ???
1807  71 13     ADC (13),Y
1809  E4 71     CPX 71
180B  FE 00 87  INC 8700,X
180E  A0 64     LDY #64
1810  46 BA     LSR BA
1812  03          ???
1813  0A        ASL A
1814  BD 10 FC  LDA FC10,X
1817  12          ???
1818  A9 D5     LDA #D5
181A  A3          ???
181B  70 11     BVS 182E
181D  F5 2B     SBC 2B,X
181F  11 05     ORA (05),Y
1821  CD 98 40  CMP 4098
1824  C3          ???
1825  F6 1D     INC 1D,X
1827  CE C3 83  DEC 83C3
182A  2B          ???
182B  E0 40     CPX #40
182D  A0 B8     LDY #B8
182F  50 E8     BVC 1819
1831  99 1E 50  STA 501E,Y
1834  0A        ASL A
1835  7C          ???
1836  1E 5C 60  ASL 605C,X
1839  6D 6D 03  ADC 036D
183C  04          ???
183D  A9 AA     LDA #AA
183F  42          ???
1840  EA        NOP 
1841  41 8C     EOR (8C,X)
1843  03          ???
1844  CB          ???
1845  C0 41     CPY #41
1847  E5 F0     SBC F0
1849  AA        TAX 
184A  A4 0B     LDY 0B
184C  13          ???
184D  2D 0D 06  AND 060D
1850  E3          ???
1851  34          ???
1852  D4          ???
1853  04          ???
1854  68        PLA 
1855  00        BRK 
1856  C3          ???
1857  FF          ???
1858  43          ???
1859  DB          ???
185A  5A          ???
185B  81 69     STA (69,X)
185D  7C          ???
185E  02          ???
185F  4B          ???
1860  C1 54     CMP (54,X)
1862  E8        INX 
1863  E9 15     SBC #15
1865  F6 43     INC 43,X
1867  39 12 A2  AND A212,Y
186A  E0 19     CPX #19
186C  7F          ???
186D  00        BRK 
186E  22          ???
186F  7C          ???
1870  A9 C6     LDA #C6
1872  70 81     BVS 17F5
1874  F0 89     BEQ 17FF
1876  8C C0 97  STY 97C0
1879  32          ???
187A  C8        INY 
187B  A5 95     LDA 95
187D  F6 71     INC 71,X
187F  D5 9C     CMP 9C,X
1881  32          ???
1882  4A        LSR A
1883  F8        SED 
1884  17          ???
1885  3E 42 25  ROL 2542,X
1888  19 5C 2F  ORA 2F5C,Y
188B  C1 DF     CMP (DF,X)
188D  DA          ???
188E  B0 AA     BCS 183A
1890  42          ???
1891  A9 78     LDA #78
1893  B0 1F     BCS 18B4
1895  C2          ???
1896  77          ???
1897  35 EA     AND EA,X
1899  75 A1     ADC A1,X
189B  09 37     ORA #37
189D  0D 7F 9B  ORA 9B7F
18A0  07          ???
18A1  C2          ???
18A2  5C          ???
18A3  B1 60     LDA (60),Y
18A5  95 6A     STA 6A,X
18A7  7F          ???
18A8  F1 FF     SBC (FF),Y
18AA  67          ???
18AB  ED 2D 80  SBC 802D
18AE  A4 AC     LDY AC
18B0  4D 33 D0  EOR D033
18B3  05 1F     ORA 1F
18B5  BC 7D 11  LDY 117D,X
18B8  0A        ASL A
18B9  F0 0A     BEQ 18C5
18BB  92          ???
18BC  AE 72 26  LDX 2672
18BF  7F          ???
18C0  26 57     ROL 57
18C2  23          ???
18C3  09 11     ORA #11
18C5  91 07     STA (07),Y
18C7  79 8E 78  ADC 788E,Y
18CA  0E 50 02  ASL 0250
18CD  DD C1 CE  CMP CEC1,X
18D0  7F          ???
18D1  C0 43     CPY #43
18D3  11 AC     ORA (AC),Y
18D5  80          ???
18D6  40        RTI 
18D7  31 11     AND (11),Y
18D9  95 B1     STA B1,X
18DB  55 31     EOR 31,X
18DD  3F          ???
18DE  22          ???
18DF  01 FC     ORA (FC,X)
18E1  B3          ???
18E2  0C          ???
18E3  5F          ???
18E4  00        BRK 
18E5  69 23     ADC #23
18E7  09 57     ORA #57
18E9  E3          ???
18EA  07          ???
18EB  2B          ???
18EC  D7          ???
18ED  11 8F     ORA (8F),Y
18EF  80          ???
18F0  2B          ???
18F1  1E 11 D6  ASL D611,X
18F4  FC          ???
18F5  00        BRK 
18F6  14          ???
18F7  16 52     ASL 52,X
18F9  82          ???
18FA  83          ???
18FB  D6 02     DEC 02,X
18FD  74          ???
18FE  81 00     STA (00,X)
1900  70 CC     BVS 18CE
1902  10 7B     BPL 197F
1904  0A        ASL A
1905  68        PLA 
1906  7F          ???
1907  87          ???
1908  B8        CLV 
1909  0A        ASL A
190A  2F          ???
190B  90 A5     BCC 18B2
190D  36 25     ROL 25,X
190F  FF          ???
1910  F0 9C     BEQ 18AE
1912  CA        DEX 
1913  A8        TAY 
1914  13          ???
1915  80          ???
1916  A6 70     LDX 70
1918  26 94     ROL 94
191A  FF          ???
191B  15 89     ORA 89,X
191D  02          ???
191E  02          ???
191F  97          ???
1920  AA        TAX 
1921  56 95     LSR 95,X
1923  56 49     LSR 49,X
1925  35 55     AND 55,X
1927  70 0F     BVS 1938
1929  CF          ???
192A  FF          ???
192B  C0 D9     CPY #D9
192D  95 EA     STA EA,X
192F  00        BRK 
1930  05 A9     ORA A9
1932  40        RTI 
1933  2A        ROL A
1934  8E A4 C1  STX C1A4
1937  25 55     AND 55
1939  5C          ???
193A  07          ???
193B  1F          ???
193C  D7          ???
193D  FC          ???
193E  F0 20     BEQ 1960
1940  97          ???
1941  62          ???
1942  7F          ???
1943  78        SEI 
1944  2C 80 D8  BIT D880
1947  C6 E2     DEC E2
1949  40        RTI 
194A  FB          ???
194B  5A          ???
194C  0A        ASL A
194D  29 FF     AND #FF
194F  D1 68     CMP (68),Y
1951  E6 32     INC 32
1953  2B          ???
1954  E1 E8     SBC (E8,X)
1956  A0 49     LDY #49
1958  0F          ???
1959  80          ???
195A  8C 19 88  STY 8819
195D  0C          ???
195E  7D 46 08  ADC 0846,X
1961  99 17 4F  STA 4F17,Y
1964  D2          ???
1965  0C          ???
1966  06 CE     ASL CE
1968  F2          ???
1969  4E 2D EE  LSR EE2D
196C  30 CA     BMI 1938
196E  32          ???
196F  E2          ???
1970  5A          ???
1971  09 24     ORA #24
1973  2F          ???
1974  56 1B     LSR 1B,X
1976  7A          ???
1977  18        CLC 
1978  3F          ???
1979  8E 15 46  STX 4615
197C  DB          ???
197D  3E DD 09  ROL 09DD,X
1980  AA        TAX 
1981  29 07     AND #07
1983  0F          ???
1984  49 17     EOR #17
1986  FF          ???
1987  8A        TXA 
1988  D1 B3     CMP (B3),Y
198A  07          ???
198B  A2 A1     LDX #A1
198D  00        BRK 
198E  B9 C0 02  LDA 02C0,Y
1991  9D F9 05  STA 05F9,X
1994  C8        INY 
1995  E8        INX 
1996  E0 6F     CPX #6F
1998  27          ???
1999  DE F4 AD  DEC ADF4,X
199C  F2          ???
199D  18        CLC 
199E  69 71     ADC #71
19A0  9A        TXS 
19A1  51 2F     EOR (2F),Y
19A3  DA          ???
19A4  86 B7     STX B7
19A6  90 F1     BCC 1999
19A8  B7          ???
19A9  88        DEY 
19AA  2F          ???
19AB  1B          ???
19AC  A0 08     LDY #08
19AE  BE BA 8F  LDX 8FBA,Y
19B1  1F          ???
19B2  AD E2 CD  LDA CDE2
19B5  5A          ???
19B6  F0 FB     BEQ 19B3
19B8  8E 67 21  STX 2167
19BB  9E          ???
19BC  EA        NOP 
19BD  16 86     ASL 86,X
19BF  88        DEY 
19C0  10 EB     BPL 19AD
19C2  70 FF     BVS 19C3
19C4  85 02     STA 02
19C6  DF          ???
19C7  CF          ???
19C8  0D E5 D8  ORA D8E5
19CB  AD CC BD  LDA BDCC
19CE  3E 09 AC  ROL AC09,X
19D1  BD 09 28  LDA 2809,X
19D4  CB          ???
19D5  C1 A6     CMP (A6,X)
19D7  1B          ???
19D8  59 B4 19  EOR 19B4,Y
19DB  20 A4 1E  JSR 1EA4
19DE  5F          ???
19DF  6A        ROR A
19E0  B7          ???
19E1  DD 68 5C  CMP 5C68,X
19E4  8D 12 D0  STA D012
19E7  4C 81 EA  JMP EA81
19EA  7F          ???
19EB  30 59     BMI 1A46
19ED  00        BRK 
19EE  4B          ???
19EF  46 34     LSR 34
19F1  5A          ???
19F2  8E 51 0A  STX 0A51
19F5  1A          ???
19F6  20 0C 01  JSR 010C
19F9  20 07 88  JSR 8807
19FC  0F          ???
19FD  24 0C     BIT 0C
19FF  12          ???
1A00  08        PHP 
1A01  09 04     ORA #04
1A03  0B          ???
1A04  82          ???
1A05  09 43     ORA #43
1A07  0B          ???
1A08  78        SEI 
1A09  00        BRK 
1A0A  86 4E     STX 4E
1A0C  4F          ???
1A0D  EA        NOP 
1A0E  2E 55 FA  ROL FA55
1A11  E9 C8     SBC #C8
1A13  DD 15 9A  CMP 9A15,X
1A16  EC 5F 80  CPX 805F
1A19  AB          ???
1A1A  1F          ???
1A1B  8B          ???
1A1C  F0 34     BEQ 1A52
1A1E  4C 63 B6  JMP B663
1A21  53          ???
1A22  CC 31 E9  CPY E931
1A25  35 84     AND 84,X
1A27  17          ???
1A28  B5 50     LDA 50,X
1A2A  A4 8C     LDY 8C
1A2C  D9 63 A1  CMP A163,Y
1A2F  C2          ???
1A30  93          ???
1A31  46 67     LSR 67
1A33  1C          ???
1A34  17          ???
1A35  2E 57 90  ROL 9057
1A38  76 02     ROR 02,X
1A3A  2B          ???
1A3B  82          ???
1A3C  EA        NOP 
1A3D  0A        ASL A
1A3E  86 43     STX 43
1A40  73          ???
1A41  59 46 33  EOR 3346,Y
1A44  8B          ???
1A45  C8        INY 
1A46  30 65     BMI 1AAD
1A48  ED CA 59  SBC 59CA
1A4B  59 AA 57  EOR 57AA,Y
1A4E  36 B2     ROL B2,X
1A50  82          ???
1A51  89          ???
1A52  21 8E     AND (8E,X)
1A54  AC DD 4B  LDY 4BDD
1A57  35 0E     AND 0E,X
1A59  05 58     ORA 58
1A5B  88        DEY 
1A5C  4A        LSR A
1A5D  C4 F3     CPY F3
1A5F  49 69     EOR #69
1A61  C3          ???
1A62  05 25     ORA 25
1A64  55 65     EOR 65,X
1A66  58        CLI 
1A67  53          ???
1A68  82          ???
1A69  8A        TXA 
1A6A  58        CLI 
1A6B  2B          ???
1A6C  30 11     BMI 1A7F
1A6E  87          ???
1A6F  2A        ROL A
1A70  FA          ???
1A71  23          ???
1A72  4E 4D 9E  LSR 9E4D
1A75  8F          ???
1A76  40        RTI 
1A77  D1 4F     CMP (4F),Y
1A79  A9 DB     LDA #DB
1A7B  13          ???
1A7C  42          ???
1A7D  4A        LSR A
1A7E  B3          ???
1A7F  36 58     ROL 58,X
1A81  EB          ???
1A82  52          ???
1A83  49 46     EOR #46
1A85  C3          ???
1A86  D5 89     CMP 89,X
1A88  55 A8     EOR A8,X
1A8A  8C 15 5A  STY 5A15
1A8D  76 48     ROR 48,X
1A8F  9A        TXS 
1A90  CF          ???
1A91  33          ???
1A92  75 DB     ADC DB,X
1A94  2D 85 D9  AND D985
1A97  CC 1A B4  CPY B41A
1A9A  3D D6 1F  AND 1FD6,X
1A9D  3C          ???
1A9E  53          ???
1A9F  4A        LSR A
1AA0  5F          ???
1AA1  40        RTI 
1AA2  63          ???
1AA3  36 2E     ROL 2E,X
1AA5  1C          ???
1AA6  20 C0 68  JSR 68C0
1AA9  42          ???
1AAA  4D B6 D0  EOR D0B6
1AAD  54          ???
1AAE  46 F6     LSR F6
1AB0  52          ???
1AB1  2B          ???
1AB2  99 F7 4C  STA 4CF7,Y
1AB5  04          ???
1AB6  B3          ???
1AB7  4C F5 55  JMP 55F5
1ABA  36 48     ROL 48,X
1ABC  34          ???
1ABD  1D 11 8B  ORA 8B11,X
1AC0  26 47     ROL 47
1AC2  49 CB     EOR #CB
1AC4  45 44     EOR 44
1AC6  28        PLP 
1AC7  41 4D     EOR (4D,X)
1AC9  33          ???
1ACA  53          ???
1ACB  CA        DEX 
1ACC  59 42 8C  EOR 8C42,Y
1ACF  F4          ???
1AD0  50 EC     BVC 1ABE
1AD2  4E 58 6F  LSR 6F58
1AD5  48        PHA 
1AD6  A9 4B     LDA #4B
1AD8  55 AB     EOR AB,X
1ADA  EC 57 17  CPX 1757
1ADD  3C          ???
1ADE  20 C6 82  JSR 82C6
1AE1  43          ???
1AE2  95 C6     STA C6,X
1AE4  FC          ???
1AE5  F8        SED 
1AE6  8A        TXA 
1AE7  4C 47 65  JMP 6547
1AEA  45 B6     EOR B6
1AEC  91 4A     STA (4A),Y
1AEE  58        CLI 
1AEF  59 BC 4F  EOR 4FBC,Y
1AF2  DA          ???
1AF3  2C D8 D2  BIT D2D8
1AF6  53          ???
1AF7  2B          ???
1AF8  6B          ???
1AF9  54          ???
1AFA  86 43     STX 43
1AFC  FE 0B F0  INC F00B,X
1AFF  AA        TAX 
1B00  44          ???
1B01  41 52     EOR (52,X)
1B03  5C          ???
1B04  2E 12 20  ROL 2012
1B07  83          ???
1B08  2E 02 FD  ROL FD02
1B0B  80          ???
1B0C  78        SEI 
1B0D  12          ???
1B0E  2E D0 A2  ROL A2D0
1B11  FB          ???
1B12  9A        TXS 
1B13  F9 84 FF  SBC FF84,Y
1B16  B8        CLV 
1B17  DC          ???
1B18  9C          ???
1B19  C7          ???
1B1A  EE 58 62  INC 6258
1B1D  40        RTI 
1B1E  20 69 50  JSR 5069
1B21  AD B1 0F  LDA 0FB1
1B24  8D EE 9D  STA 9DEE
1B27  C9 68     CMP #68
1B29  03          ???
1B2A  5D 0E 4C  EOR 4C0E,X
1B2D  DC          ???
1B2E  38        SEC 
1B2F  38        SEC 
1B30  1F          ???
1B31  7F          ???
1B32  00        BRK 
1B33  00        BRK 
1B34  92          ???
1B35  02          ???
1B36  03          ???
1B37  CB          ???
1B38  04          ???
1B39  2C 05 B2  BIT B205
1B3C  06 07     ASL 07
1B3E  CB          ???
1B3F  08        PHP 
1B40  2C 09 B2  BIT B209
1B43  0A        ASL A
1B44  0B          ???
1B45  CB          ???
1B46  0C          ???
1B47  2C 0D B2  BIT B20D
1B4A  0E 0F CB  ASL CB0F
1B4D  10 2C     BPL 1B7B
1B4F  11 B2     ORA (B2),Y
1B51  12          ???
1B52  13          ???
1B53  CB          ???
1B54  14          ???
1B55  2C 15 B2  BIT B215
1B58  16 17     ASL 17,X
1B5A  CB          ???
1B5B  18        CLC 
1B5C  2C 19 B2  BIT B219
1B5F  1A          ???
1B60  1B          ???
1B61  CB          ???
1B62  1C          ???
1B63  2C 1D B2  BIT B21D
1B66  1E 1F CB  ASL CB1F,X
1B69  20 2C 21  JSR 212C
1B6C  B2          ???
1B6D  22          ???
1B6E  23          ???
1B6F  CB          ???
1B70  24 2C     BIT 2C
1B72  25 B2     AND B2
1B74  26 27     ROL 27
1B76  CB          ???
1B77  28        PLP 
1B78  2C 29 B2  BIT B229
1B7B  2A        ROL A
1B7C  2B          ???
1B7D  CB          ???
1B7E  2C 2C 2D  BIT 2D2C
1B81  B2          ???
1B82  2E 2F CB  ROL CB2F
1B85  30 2C     BMI 1BB3
1B87  31 B2     AND (B2),Y
1B89  32          ???
1B8A  33          ???
1B8B  01 CB     ORA (CB,X)
1B8D  78        SEI 
1B8E  87          ???
1B8F  B5 76     LDA 76,X
1B91  AD 8C 0B  LDA 0B8C
1B94  14          ???
1B95  5C          ???
1B96  A5 23     LDA 23
1B98  B5 78     LDA 78,X
1B9A  15 03     ORA 03,X
1B9C  6E D5 A1  ROR A1D5
1B9F  25 DB     AND DB
1BA1  D4          ???
1BA2  56 AC     LSR AC,X
1BA4  40        RTI 
1BA5  08        PHP 
1BA6  7B          ???
1BA7  4B          ???
1BA8  93          ???
1BA9  32          ???
1BAA  80          ???
1BAB  E3          ???
1BAC  4C A4 34  JMP 34A4
1BAF  F1 DC     SBC (DC),Y
1BB1  4B          ???
1BB2  09 5E     ORA #5E
1BB4  10 03     BPL 1BB9
1BB6  2C 0D DD  BIT DD0D
1BB9  DF          ???
1BBA  EA        NOP 
1BBB  AF          ???
1BBC  81 4D     STA (4D,X)
1BBE  38        SEC 
1BBF  9F          ???
1BC0  46 DB     LSR DB
1BC2  00        BRK 
1BC3  36 02     ROL 02,X
1BC5  0E 03 17  ASL 1703
1BC8  F9 28 04  SBC 0428,Y
1BCB  66 B7     ROR B7
1BCD  21 F1     AND (F1,X)
1BCF  23          ???
1BD0  35 83     AND 83,X
1BD2  48        PHA 
1BD3  68        PLA 
1BD4  86 28     STX 28
1BD6  1A          ???
1BD7  B5 34     LDA 34,X
1BD9  F1 9C     SBC (9C),Y
1BDB  4C 06 39  JMP 3906
1BDE  85 F2     STA F2
1BE0  AA        TAX 
1BE1  0E F7 D5  ASL D5F7
1BE4  17          ???
1BE5  AB          ???
1BE6  F1 56     SBC (56),Y
1BE8  28        PLP 
1BE9  AD ED 5A  LDA 5AED
1BEC  2A        ROL A
1BED  B5 08     LDA 08,X
1BEF  6A        ROR A
1BF0  2C BE C9  BIT C9BE
1BF3  32          ???
1BF4  99 63 91  STA 9163,Y
1BF7  05 A5     ORA A5
1BF9  23          ???
1BFA  A4 A2     LDY A2
1BFC  24 8E     BIT 8E
1BFE  76 37     ROR 37,X
1C00  4C 50 35  JMP 3550
1C03  BF          ???
1C04  C2          ???
1C05  2B          ???
1C06  0D 04 69  ORA 6904
1C09  50 9A     BVC 1BA5
1C0B  1F          ???
1C0C  2B          ???
1C0D  4C A6 F0  JMP F0A6
1C10  E0 2D     CPX #2D
1C12  D0 15     BNE 1C29
1C14  7F          ???
1C15  2A        ROL A
1C16  99 39 B6  STA B639,Y
1C19  18        CLC 
1C1A  3D C5 81  AND 81C5,X
1C1D  2D 56 74  AND 7456
1C20  3E 95 29  ROL 2995,X
1C23  82          ???
1C24  79 36 23  ADC 2336,Y
1C27  9A        TXS 
1C28  05 9A     ORA 9A
1C2A  1A          ???
1C2B  5C          ???
1C2C  88        DEY 
1C2D  78        SEI 
1C2E  7B          ???
1C2F  2F          ???
1C30  BB          ???
1C31  0E B3 8C  ASL 8CB3
1C34  F7          ???
1C35  62          ???
1C36  FA          ???
1C37  72          ???
1C38  65 D3     ADC D3
1C3A  4A        LSR A
1C3B  8D D6 FC  STA FCD6
1C3E  E0 7D     CPX #7D
1C40  FB          ???
1C41  14          ???
1C42  AD C3 35  LDA 35C3
1C45  5E 0F 7B  LSR 7B0F,X
1C48  D6 8A     DEC 8A,X
1C4A  E4 AE     CPX AE
1C4C  D1 5F     CMP (5F),Y
1C4E  25 9D     AND 9D
1C50  57          ???
1C51  50 71     BVC 1CC4
1C53  77          ???
1C54  F4          ???
1C55  E0 7C     CPX #7C
1C57  3B          ???
1C58  5B          ???
1C59  EA        NOP 
1C5A  A5 F0     LDA F0
1C5C  71 A4     ADC (A4),Y
1C5E  AA        TAX 
1C5F  F1 B5     SBC (B5),Y
1C61  61 39     ADC (39,X)
1C63  8D F2 1B  STA 1BF2
1C66  9F          ???
1C67  7F          ???
1C68  F3          ???
1C69  C8        INY 
1C6A  0B          ???
1C6B  96 0A     STX 0A,Y
1C6D  31 95     AND (95),Y
1C6F  3A          ???
1C70  89          ???
1C71  2A        ROL A
1C72  43          ???
1C73  F6 B4     INC B4,X
1C75  01 D0     ORA (D0,X)
1C77  25 F6     AND F6
1C79  86 A3     STX A3
1C7B  B7          ???
1C7C  1A          ???
1C7D  13          ???
1C7E  8C CD 1B  STY 1BCD
1C81  B4 9D     LDY 9D,X
1C83  4C FB 04  JMP 04FB
1C86  E0 58     CPX #58
1C88  D5 0A     CMP 0A,X
1C8A  DA          ???
1C8B  6C 8E B5  JMP (B58E)
1C8E  96 05     STX 05,Y
1C90  13          ???
1C91  B8        CLV 
1C92  A5 D2     LDA D2
1C94  98        TYA 
1C95  A5 9A     LDA 9A
1C97  35 16     AND 16,X
1C99  BB          ???
1C9A  D3          ???
1C9B  7B          ???
1C9C  E2          ???
1C9D  9A        TXS 
1C9E  AD 57 72  LDA 7257
1CA1  0D 75 A3  ORA A375
1CA4  20 22 65  JSR 6522
1CA7  4C 5A 36  JMP 365A
1CAA  E0 1E     CPX #1E
1CAC  D0 10     BNE 1CBE
1CAE  FF          ???
1CAF  48        PHA 
1CB0  97          ???
1CB1  C4 61     CPY 61
1CB3  94 C4     STY C4,X
1CB5  F3          ???
1CB6  D7          ???
1CB7  C3          ???
1CB8  4F          ???
1CB9  90 8A     BCC 1C45
1CBB  18        CLC 
1CBC  60        RTS 
1CBD  0A        ASL A
1CBE  AA        TAX 
1CBF  EF          ???
1CC0  BD F4 BC  LDA BCF4,X
1CC3  01 34     ORA (34,X)
1CC5  57          ???
1CC6  C7          ???
1CC7  60        RTS 
1CC8  AE 85 B2  LDX B285
1CCB  A0 08     LDY #08
1CCD  5B          ???
1CCE  95 15     STA 15,X
1CD0  FB          ???
1CD1  50 F9     BVC 1CCC
1CD3  46 F2     LSR F2
1CD5  BF          ???
1CD6  4B          ???
1CD7  09 10     ORA #10
1CD9  0D AD 49  ORA 49AD
1CDC  30 CF     BMI 1CAD
1CDE  C8        INY 
1CDF  A9 C0     LDA #C0
1CE1  C1 41     CMP (41,X)
1CE3  69 C1     ADC #C1
1CE5  4C 36 CF  JMP CF36
1CE8  92          ???
1CE9  88        DEY 
1CEA  D0 D7     BNE 1CC3
1CEC  8E CE 70  STX 70CE
1CEF  FB          ???
1CF0  8F          ???
1CF1  03          ???
1CF2  74          ???
1CF3  34          ???
1CF4  E4 8F     CPX 8F
1CF6  F1 09     SBC (09),Y
1CF8  20 9F DA  JSR DA9F
1CFB  0B          ???
1CFC  EC 4B AD  CPX AD4B
1CFF  6A        ROR A
1D00  DE 05 18  DEC 1805,X
1D03  69 B3     ADC #B3
1D05  2F          ???
1D06  8D DF F9  STA F9DF
1D09  B7          ???
1D0A  8D 34 0C  STA 0C34
1D0D  68        PLA 
1D0E  B8        CLV 
1D0F  56 86     LSR 86,X
1D11  12          ???
1D12  37          ???
1D13  CE 8C 7F  DEC 7F8C
1D16  E3          ???
1D17  47          ???
1D18  2F          ???
1D19  F6 4B     INC 4B,X
1D1B  8E 23 D8  STX D823
1D1E  A5 F1     LDA F1
1D20  8D 7D 60  STA 607D
1D23  99 08 F4  STA F408,Y
1D26  DA          ???
1D27  D0 0D     BNE 1D36
1D29  8E 7A C5  STX C57A
1D2C  8C AF 65  STY 65AF
1D2F  7E CA E6  ROR E6CA,X
1D32  D2          ???
1D33  27          ???
1D34  C4 7A     CPY 7A
1D36  3F          ???
1D37  EE 33 28  INC 2833
1D3A  D3          ???
1D3B  22          ???
1D3C  60        RTS 
1D3D  DE 90 1A  DEC 1A90,X
1D40  20 D1 63  JSR 63D1
1D43  A8        TAY 
1D44  A9 99     LDA #99
1D46  8D 09 37  STA 3709
1D49  BF          ???
1D4A  2F          ???
1D4B  17          ???
1D4C  02          ???
1D4D  8B          ???
1D4E  07          ???
1D4F  8C B1 36  STY 36B1
1D52  A6 CF     LDX CF
1D54  F1 2C     SBC (2C),Y
1D56  CB          ???
1D57  50 FB     BVC 1D54
1D59  58        CLI 
1D5A  38        SEC 
1D5B  4F          ???
1D5C  3A          ???
1D5D  34          ???
1D5E  E9 31     SBC #31
1D60  90 04     BCC 1D66
1D62  29 07     AND #07
1D64  BC F2 B6  LDY B6F2,X
1D67  D4          ???
1D68  B5 54     LDA 54,X
1D6A  65 49     ADC 49
1D6C  CF          ???
1D6D  F0 32     BEQ 1DA1
1D6F  AD C4 50  LDA 50C4
1D72  4A        LSR A
1D73  05 4D     ORA 4D
1D75  C4 8E     CPY 8E
1D77  DD 95 49  CMP 4995,X
1D7A  8C 3C B1  STY B13C
1D7D  3F          ???
1D7E  79 C9 C2  ADC C2C9,Y
1D81  79 EB 60  ADC 60EB,Y
1D84  79 02 A2  ADC A202,Y
1D87  03          ???
1D88  CF          ???
1D89  FD 95 14  SBC 1495,X
1D8C  06 2A     ASL 2A
1D8E  B2          ???
1D8F  24 4F     BIT 4F
1D91  85 AF     STA AF
1D93  E8        INX 
1D94  98        TYA 
1D95  3B          ???
1D96  29 0F     AND #0F
1D98  F0 13     BEQ 1DAD
1D9A  FF          ???
1D9B  4A        LSR A
1D9C  A6 F6     LDX F6
1D9E  2A        ROL A
1D9F  DE 6E 10  DEC 106E,X
1DA2  FA          ???
1DA3  8F          ???
1DA4  11 AA     ORA (AA),Y
1DA6  89          ???
1DA7  23          ???
1DA8  85 52     STA 52
1DAA  0E 8A 63  ASL 638A
1DAD  77          ???
1DAE  B1 59     LDA (59),Y
1DB0  E3          ???
1DB1  83          ???
1DB2  99 19 76  STA 7619,Y
1DB5  C8        INY 
1DB6  C0 34     CPY #34
1DB8  7B          ???
1DB9  D3          ???
1DBA  DE 17 F0  DEC F017,X
1DBD  30 E8     BMI 1DA7
1DBF  9C          ???
1DC0  B3          ???
1DC1  1E 17 7E  ASL 7E17,X
1DC4  E0 5D     CPX #5D
1DC6  01 90     ORA (90,X)
1DC8  14          ???
1DC9  FB          ???
1DCA  A7          ???
1DCB  4A        LSR A
1DCC  4B          ???
1DCD  04          ???
1DCE  6D 6A A3  ADC A36A
1DD1  24 26     BIT 26
1DD3  F7          ???
1DD4  DC          ???
1DD5  6D F2 85  ADC 85F2
1DD8  DC          ???
1DD9  A5 F6     LDA F6
1DDB  60        RTS 
1DDC  CA        DEX 
1DDD  EF          ???
1DDE  15 F5     ORA F5,X
1DE0  88        DEY 
1DE1  B1 F4     LDA (F4),Y
1DE3  91 1F     STA (1F),Y
1DE5  98        TYA 
1DE6  6D F8 8A  ADC 8AF8
1DE9  2B          ???
1DEA  4D 56 24  EOR 2456
1DED  0D 2F D0  ORA D02F
1DF0  33          ???
1DF1  1E C6 F9  ASL F9C6,X
1DF4  8F          ???
1DF5  9D 36 90  STA 9036,X
1DF8  E6 AF     INC AF
1DFA  C8        INY 
1DFB  D6 55     DEC 55,X
1DFD  D6 F0     DEC F0,X
1DFF  FA          ???
1E00  C0 11     CPY #11
1E02  B0 D3     BCS 1DD7
1E04  BE 05 FF  LDX FF05,Y
1E07  07          ???
1E08  39 61 85  AND 8561,Y
1E0B  0D CD CE  ORA CECD
1E0E  6D 8D 48  ADC 488D
1E11  D0 06     BNE 1E19
1E13  EF          ???
1E14  33          ???
1E15  C0 04     CPY #04
1E17  90 3E     BCC 1E57
1E19  65 77     ADC 77
1E1B  1F          ???
1E1C  FF          ???
1E1D  4A        LSR A
1E1E  A4 81     LDY 81
1E20  69 A8     ADC #A8
1E22  5B          ???
1E23  A5 6A     LDA 6A
1E25  E5 F3     SBC F3
1E27  FF          ???
1E28  5E B0 D4  LSR D4B0,X
1E2B  E3          ???
1E2C  BE 06 E3  LDX E306,Y
1E2F  20 56 EB  JSR EB56
1E32  37          ???
1E33  F6 3A     INC 3A,X
1E35  F6 03     INC 03,X
1E37  FD E6 86  SBC 86E6,X
1E3A  18        CLC 
1E3B  A6 F9     LDX F9
1E3D  A6 F4     LDX F4
1E3F  A5 F7     LDA F7
1E41  79 6E 38  ADC 386E,Y
1E44  65 FA     ADC FA
1E46  FF          ???
1E47  85 F5     STA F5
1E49  A4 F3     LDY F3
1E4B  FD 27 90  SBC 9027,X
1E4E  8D 32 46  STA 4632
1E51  02          ???
1E52  04          ???
1E53  04          ???
1E54  30 20     BMI 1E76
1E56  10 5E     BPL 1EB6
1E58  8F          ???
1E59  52          ???
1E5A  C0 78     CPY #78
1E5C  0B          ???
1E5D  32          ???
1E5E  0D C1 5A  ORA 5AC1
1E61  FE D2 B4  INC B4D2,X
1E64  5A          ???
1E65  FA          ???
1E66  D2          ???
1E67  38        SEC 
1E68  5A          ???
1E69  15 34     ORA 34,X
1E6B  8D FB 27  STA 27FB
1E6E  FF          ???
1E6F  F6 2F     INC 2F,X
1E71  F6 00     INC 00,X
1E73  58        CLI 
1E74  D4          ???
1E75  9C          ???
1E76  8A        TXA 
1E77  98        TYA 
1E78  CA        DEX 
1E79  A5 55     LDA 55
1E7B  AC 27 35  LDY 3527
1E7E  29 EE     AND #EE
1E80  19 4F 70  ORA 704F,Y
1E83  E3          ???
1E84  2C 28 8B  BIT 8B28
1E87  A8        TAY 
1E88  25 AA     AND AA
1E8A  68        PLA 
1E8B  40        RTI 
1E8C  97          ???
1E8D  DA          ???
1E8E  60        RTS 
1E8F  59 A2 02  EOR 02A2,Y
1E92  83          ???
1E93  95 86     STA 86,X
1E95  E8        INX 
1E96  FE 86 4C  INC 4C86,X
1E99  52          ???
1E9A  16 7C     ASL 7C,X
1E9C  C4 35     CPY 35
1E9E  00        BRK 
1E9F  95 80     STA 80,X
1EA1  20 DE 10  JSR 10DE
1EA4  F2          ???
1EA5  60        RTS 
1EA6  7C          ???
1EA7  60        RTS 
1EA8  02          ???
1EA9  4C AA 08  JMP 08AA
1EAC  78        SEI 
1EAD  AD 7F 12  LDA 127F
1EB0  89          ???
1EB1  BD 92 0E  LDA 0E92,X
1EB4  44          ???
1EB5  15 D0     ORA D0,X
1EB7  D3          ???
1EB8  18        CLC 
1EB9  D4          ???
1EBA  75 37     ADC 37,X
1EBC  85 7B     STA 7B
1EBE  7F          ???
1EBF  59 89 14  EOR 1489,Y
1EC2  0D F5 08  ORA 08F5
1EC5  31 32     AND (32),Y
1EC7  0E 7A DC  ASL DC7A
1ECA  89          ???
1ECB  8D 0F DD  STA DD0F
1ECE  E0 B4     CPX #B4
1ED0  E8        INX 
1ED1  55 28     EOR 28,X
1ED3  02          ???
1ED4  98        TYA 
1ED5  99 99 00  STA 0099,Y
1ED8  3E B9 AC  ROL ACB9,X
1EDB  3E 2B 3C  ROL 3C2B,X
1EDE  63          ???
1EDF  03          ???
1EE0  86 B2     STX B2
1EE2  7A          ???
1EE3  B3          ???
1EE4  A9 08     LDA #08
1EE6  DC          ???
1EE7  A0 54     LDY #54
1EE9  99 82 5A  STA 5A82,Y
1EEC  8C 84 56  STY 5684
1EEF  8E 88 57  STX 5788
1EF2  AD EE 36  LDA 36EE
1EF5  D7          ???
1EF6  8D A6 02  STA 02A6
1EF9  E5 8A     SBC 8A
1EFB  FF          ???
1EFC  65 53     ADC 53
1EFE  E4 66     CPX 66
1F00  7E F1 37  ROR 37F1,X
1F03  EE 80 9F  INC 9F80
1F06  BC 06 FC  LDY FC06,X
1F09  9A        TXS 
1F0A  E6 F7     INC F7
1F0C  AA        TAX 
1F0D  40        RTI 
1F0E  47          ???
1F0F  06 54     ASL 54
1F11  8E 00 DC  STX DC00
1F14  04          ???
1F15  A0 34     LDY #34
1F17  DD 92 AD  CMP AD92,X
1F1A  C6 01     DEC 01
1F1C  ED A2 38  SBC 38A2
1F1F  A9 7F     LDA #7F
1F21  AF          ???
1F22  20 0C 35  JSR 350C
1F25  4C 42 41  JMP 4142
1F28  85 FF     STA FF
1F2A  86 9A     STX 9A
1F2C  8C DB 39  STY 39DB
1F2F  A0 5B     LDY #5B
1F31  98        TYA 
1F32  AD 32 87  LDA 8732
1F35  FB          ???
1F36  A1 E6     LDA (E6,X)
1F38  A6 FF     LDX FF
1F3A  CB          ???
1F3B  E0 EE     CPX #EE
1F3D  03          ???
1F3E  27          ???
1F3F  48        PHA 
1F40  A9 20     LDA #20
1F42  8E 25 54  STX 5425
1F45  AA        TAX 
1F46  26 BE     ROL BE
1F48  68        PLA 
1F49  8D 11 1C  STA 1C11
1F4C  BF          ???
1F4D  60        RTS 
1F4E  82          ???
1F4F  DB          ???
1F50  00        BRK 
1F51  04          ???
1F52  EF          ???
1F53  80          ???
1F54  CB          ???
1F55  64          ???
1F56  B9 00 94  LDA 9400,Y
1F59  BA        TSX 
1F5A  16 17     ASL 17,X
1F5C  C9 74     CMP #74
1F5E  3D 26 60  AND 6026,X
1F61  80          ???
1F62  9B          ???
1F63  90 10     BCC 1F75
1F65  A9 D7     LDA #D7
1F67  4C 8A 38  JMP 388A
1F6A  4A        LSR A
1F6B  FC          ???
1F6C  DC          ???
1F6D  C9 FF     CMP #FF
1F6F  F0 F9     BEQ 1F6A
1F71  D0 EB     BNE 1F5E
1F73  60        RTS 
1F74  FF          ???
1F75  1D 4E 57  ORA 574E,X
1F78  D7          ???
1F79  C3          ???
1F7A  43          ???
1F7B  20 48 54  JSR 5448
1F7E  33          ???
1F7F  BE 4F 79  LDX 794F,Y
1F82  2D CD 93  AND 93CD
1F85  9B          ???
1F86  CE F1 2B  DEC 2BF1
1F89  D3          ???
1F8A  35 31     AND 31,X
1F8C  ED 2E 30  SBC 302E
1F8F  D7          ???
1F90  E0 1C     CPX #1C
1F92  C4 A3     CPY A3
1F94  E8        INX 
1F95  56 F6     LSR F6,X
1F97  47          ???
1F98  45 A2     EOR A2
1F9A  4F          ???
1F9B  52          ???
1F9C  21 97     AND (97,X)
1F9E  95 0D     STA 0D,X
1FA0  9B          ???
1FA1  D3          ???
1FA2  43          ???
1FA3  6F          ???
1FA4  41 8A     EOR (8A,X)
1FA6  56 D2     LSR D2,X
1FA8  E8        INX 
1FA9  00        BRK 
1FAA  49 34     EOR #34
1FAC  22          ???
1FAD  09 30     ORA #30
1FAF  12          ???
1FB0  37          ???
1FB1  48        PHA 
1FB2  8A        TXA 
1FB3  35 FE     AND FE,X
1FB5  31 92     AND (92),Y
1FB7  7E 43 4D  ROR 4D43,X
1FBA  AB          ???
1FBB  46 44     LSR 44
1FBD  20 3B 42  JSR 423B
1FC0  B0 FD     BCS 1FBF
1FC2  8A        TXA 
1FC3  F0 04     BEQ 1FC9
1FC5  C9 AF     CMP #AF
1FC7  07          ???
1FC8  F4          ???
1FC9  38        SEC 
1FCA  60        RTS 
1FCB  86 57     STX 57
1FCD  CF          ???
1FCE  2A        ROL A
1FCF  75 48     ADC 48,X
1FD1  A9 5C     LDA #5C
1FD3  CE 1B 1E  DEC 1E1B
1FD6  29 AB     AND #AB
1FD8  F6 AA     INC AA,X
1FDA  A0 03     LDY #03
1FDC  BD 64 40  LDA 4064,X
1FDF  FD 1C 5D  SBC 5D1C,X
1FE2  88        DEY 
1FE3  10 F6     BPL 1FDB
1FE5  68        PLA 
1FE6  0F          ???
1FE7  BC D0 F9  LDY F9D0,X
1FEA  2F          ???
1FEB  CC D5 EA  CPY EAD5
1FEE  A9 43     LDA #43
1FF0  66 A2     ROR A2
1FF2  ED 1B BD  SBC BD1B
1FF5  03          ???
1FF6  F5 18     SBC 18,X
1FF8  65 B9     ADC B9
1FFA  85 6F     STA 6F
1FFC  57          ???
1FFD  2B          ???
1FFE  5B          ???
1FFF  BC 1F C7  LDY C71F,X
2002  42          ???
2003  F9 D5 48  SBC 48D5,Y
2006  B9 AB F8  LDA F8AB,Y
2009  DE C0 01  DEC 01C0,X
200C  C3          ???
200D  BE 64 68  LDX 6864,Y
2010  90 03     BCC 2015
2012  C8        INY 
2013  91 5A     STA (5A),Y
2015  FE 34 DB  INC DB34,X
2018  F0 B5     BEQ 1FCF
201A  C5 1D     CMP 1D
201C  0F          ???
201D  BD 2D 40  LDA 402D,X
2020  BD 9A CA  LDA CA9A,X
2023  10 F7     BPL 201C
2025  39 48 A2  AND A248,Y
2028  E0 E6     CPX #E6
202A  64          ???
202B  61 68     ADC (68,X)
202D  15 D3     ORA D3,X
202F  1A          ???
2030  72          ???
2031  C1 16     CMP (16,X)
2033  C7          ???
2034  99 A7 58  STA 58A7,Y
2037  CD 1A 72  CMP 721A
203A  1C          ???
203B  58        CLI 
203C  2D 0F 0C  AND 0C0F
203F  46 D8     LSR D8
2041  0C          ???
2042  8E 24 90  STX 9024
2045  50 E7     BVC 202E
2047  F9 CE AB  SBC ABCE,Y
204A  8A        TXA 
204B  E5 42     SBC 42
204D  85 FB     STA FB
204F  BE D7 D9  LDX D9D7,Y
2052  95 DF     STA DF,X
2054  3B          ???
2055  56 12     LSR 12,X
2057  55 32     EOR 32,X
2059  45 12     EOR 12
205B  72          ???
205C  57          ???
205D  1E EB A9  ASL A9EB,X
2060  10 B9     BPL 201B
2062  F1 DF     SBC (DF),Y
2064  72          ???
2065  4C 8C CC  JMP CC8C
2068  86 FE     STX FE
206A  EA        NOP 
206B  50 50     BVC 20BD
206D  EC 05 85  CPX 8505
2070  1B          ???
2071  0D 9C 3B  ORA 3B9C
2074  57          ???
2075  33          ???
2076  11 01     ORA (01),Y
2078  D1 A5     CMP (A5),Y
207A  FD 23 07  SBC 0723,X
207D  20 2B A4  JSR A42B
2080  23          ???
2081  18        CLC 
2082  D7          ???
2083  69 F5     ADC #F5
2085  85 AC     STA AC
2087  B1 FE     LDA (FE),Y
2089  6D D4 C8  ADC C8D4
208C  C4 FC     CPY FC
208E  7C          ???
208F  F6 D5     INC D5,X
2091  AE D2 C6  LDX C6D2
2094  FB          ???
2095  F0 09     BEQ 20A0
2097  98        TYA 
2098  DF          ???
2099  CD EB FD  CMP FDEB
209C  E6 2C     INC 2C
209E  B7          ???
209F  FB          ???
20A0  8C CE 41  STY 41CE
20A3  4A        LSR A
20A4  4D D6 B5  EOR B5D6
20A7  24 2D     BIT 2D
20A9  E9 A8     SBC #A8
20AB  68        PLA 
20AC  A2 6D     LDX #6D
20AE  EB          ???
20AF  B1 23     LDA (23),Y
20B1  8A        TXA 
20B2  57          ???
20B3  93          ???
20B4  B5 37     LDA 37,X
20B6  45 90     EOR 90
20B8  71 A5     ADC (A5),Y
20BA  BA        TSX 
20BB  C4 9C     CPY 9C
20BD  EC 4C 96  CPX 964C
20C0  63          ???
20C1  FF          ???
20C2  82          ???
20C3  70 41     BVS 2106
20C5  CC 20 44  CPY 4420
20C8  90 42     BCC 210C
20CA  48        PHA 
20CB  83          ???
20CC  24 00     BIT 00
20CE  12          ???
20CF  EA        NOP 
20D0  01 04     ORA (04,X)
20D2  20 3A 90  JSR 903A
20D5  48        PHA 
20D6  48        PHA 
20D7  45 24     EOR 24
20D9  43          ???
20DA  13          ???
20DB  52          ???
20DC  01 37     ORA (37,X)
20DE  02          ???
20DF  A0 41     LDY #41
20E1  16 20     ASL 20,X
20E3  97          ???
20E4  81 05     STA (05,X)
20E6  4E 58 3C  LSR 3C58
20E9  59 2A B6  EOR B62A,Y
20EC  D2          ???
20ED  6D 00 80  ADC 8000
20F0  FB          ???
20F1  2A        ROL A
20F2  29 08     AND #08
20F4  1D CF F7  ORA F7CF,X
20F7  68        PLA 
20F8  8D 0A 0A  STA 0A0A
20FB  68        PLA 
20FC  D4          ???
20FD  E5 13     SBC 13
20FF  CB          ???
2100  21 28     AND (28,X)
2102  06 14     ASL 14
2104  6F          ???
2105  25 94     AND 94
2107  03          ???
2108  13          ???
2109  2C 50 49  BIT 4950
210C  1A          ???
210D  02          ???
210E  40        RTI 
210F  09 00     ORA #00
2111  E1 05     SBC (05,X)
2113  07          ???
2114  03          ???
2115  05 01     ORA 01
2117  7D 3A A7  ADC A73A,X
211A  34          ???
211B  EC A9 6F  CPX 6FA9
211E  85 B9     STA B9
2120  3E 1D B0  ROL B01D,X
2123  53          ???
2124  1F          ???
2125  B4 A8     LDY A8,X
2127  69 C4     ADC #C4
2129  98        TYA 
212A  A2 03     LDX #03
212C  DD B2 1F  CMP 1FB2,X
212F  79 05 97  ADC 9705,Y
2132  F8        SED 
2133  99 79 9D  STA 9D79,Y
2136  B5 72     LDA 72,X
2138  B0 4A     BCS 2184
213A  B8        CLV 
213B  6D 8D B1  ADC B18D
213E  2B          ???
213F  A9 06     LDA #06
2141  EB          ???
2142  A2 AD     LDX #AD
2144  A0 E9     LDY #E9
2146  8A        TXA 
2147  BD B6 D9  LDA D9B6,X
214A  B9 F3 A1  LDA A1F3,Y
214D  B5 BC     LDA BC,X
214F  D0 E6     BNE 2137
2151  13          ???
2152  EE 85 BA  INC BA85
2155  E2          ???
2156  EF          ???
2157  ED 20 46  SBC 4620
215A  F6 BE     INC BE,X
215C  F8        SED 
215D  E0 58     CPX #58
215F  68        PLA 
2160  BB          ???
2161  68        PLA 
2162  6D 31 78  ADC 7831
2165  59 DD C3  EOR C3DD,Y
2168  42          ???
2169  F0 03     BEQ 216E
216B  FC          ???
216C  A9 EF     LDA #EF
216E  18        CLC 
216F  60        RTS 
2170  20 CF B4  JSR B4CF
2173  41 4C     EOR (4C,X)
2175  A5 F1     LDA F1
2177  C9 A0     CMP #A0
2179  23          ???
217A  01 4D     ORA (4D,X)
217C  DB          ???
217D  2D 9E 10  AND 109E
2180  87          ???
2181  43          ???
2182  0D FF A4  ORA A4FF
2185  CF          ???
2186  C6 E9     DEC E9
2188  FE E5 A6  INC A6E5,X
218B  34          ???
218C  FC          ???
218D  37          ???
218E  38        SEC 
218F  48        PHA 
2190  46 5E     LSR 5E
2192  52          ???
2193  2C B0 A2  BIT A2B0
2196  B1 8A     LDA (8A),Y
2198  49 44     EOR #44
219A  78        SEI 
219B  B6 0B     LDX 0B,Y
219D  77          ???
219E  11 DD     ORA (DD),Y
21A0  4C 4A D6  JMP D64A
21A3  AD 8B 02  LDA 028B
21A6  8D 8F 16  STA 168F
21A9  A0 01     LDY #01
21AB  C3          ???
21AC  0B          ???
21AD  88        DEY 
21AE  2B          ???
21AF  0C          ???
21B0  84 0D     STY 0D
21B2  97          ???
21B3  94 B1     STY B1,X
21B5  C4 70     CPY 70
21B7  D2          ???
21B8  F7          ???
21B9  E5 03     SBC 03
21BB  AD B7 0E  LDA 0EB7
21BE  C9 BA     CMP #BA
21C0  F0 40     BEQ 2202
21C2  32          ???
21C3  74          ???
21C4  24 D2     BIT D2
21C6  20 75 05  JSR 0575
21C9  A4 0F     LDY 0F
21CB  0D 8D BC  ORA BC8D
21CE  67          ???
21CF  A5 B1     LDA B1
21D1  0C          ???
21D2  F9 E2 90  SBC 90E2,Y
21D5  14          ???
21D6  9A        TXS 
21D7  55 8D     EOR 8D,X
21D9  7B          ???
21DA  14          ???
21DB  56 01     LSR 01,X
21DD  EC 2C 3C  CPX 3C2C
21E0  C6 D6     DEC D6
21E2  E0 F0     CPX #F0
21E4  12          ???
21E5  9E          ???
21E6  0B          ???
21E7  AC 3D 0C  LDY 0C3D
21EA  8C 15 A0  STY A015
21ED  C7          ???
21EE  87          ???
21EF  D0 B9     BNE 21AA
21F1  00        BRK 
21F2  58        CLI 
21F3  ED 9C 2C  SBC 2C9C
21F6  8D 5A 0B  STA 0B5A
21F9  93          ???
21FA  70 03     BVS 21FF
21FC  82          ???
21FD  58        CLI 
21FE  4C 7D 2C  JMP 2C7D
2201  78        SEI 
2202  CF          ???
2203  B0 66     BCS 226B
2205  C3          ???
2206  64          ???
2207  E3          ???
2208  B4 30     LDY 30,X
220A  E8        INX 
220B  F6 4D     INC 4D,X
220D  40        RTI 
220E  A0 F8     LDY #F8
2210  E4 AD     CPX AD
2212  4A        LSR A
2213  9B          ???
2214  27          ???
2215  C1 26     CMP (26,X)
2217  CD 93 88  CMP 8893
221A  36 48     ROL 48,X
221C  A8        TAY 
221D  C8        INY 
221E  77          ???
221F  44          ???
2220  0F          ???
2221  F5 A5     SBC A5,X
2223  8E EE 7E  STX 7EEE
2226  94 33     STY 33,X
2228  D4          ???
2229  08        PHP 
222A  A1 EA     LDA (EA,X)
222C  68        PLA 
222D  A8        TAY 
222E  63          ???
222F  51 34     EOR (34),Y
2231  2F          ???
2232  5C          ???
2233  9A        TXS 
2234  BE 13 67  LDX 6713,Y
2237  1D 10 7F  ORA 7F10,X
223A  FC          ???
223B  05 AC     ORA AC
223D  15 36     ORA 36,X
223F  4E 98 98  LSR 9898
2242  B2          ???
2243  9E          ???
2244  E2          ???
2245  D4          ???
2246  59 E7 71  EOR 71E7,Y
2249  1F          ???
224A  97          ???
224B  65 F1     ADC F1
224D  1A          ???
224E  52          ???
224F  20 21 B6  JSR B621
2252  9B          ???
2253  B9 87 0D  LDA 0D87,Y
2256  85 81     STA 81
2258  4A        LSR A
2259  14          ???
225A  CC D7 8A  CPY 8AD7
225D  D6 32     DEC 32,X
225F  E1 EB     SBC (EB,X)
2261  3C          ???
2262  8B          ???
2263  06 80     ASL 80
2265  A9 49     LDA #49
2267  C0 72     CPY #72
2269  15 A9     ORA A9,X
226B  00        BRK 
226C  E2          ???
226D  DE 41 7A  DEC 7A41,X
2270  C0 22     CPY #22
2272  33          ???
2273  48        PHA 
2274  12          ???
2275  BB          ???
2276  72          ???
2277  EA        NOP 
2278  28        PLP 
2279  0D 8C 8C  ORA 8C8C
227C  99 98 C3  STA C398,Y
227F  FC          ???
2280  29 99     AND #99
2282  09 23     ORA #23
2284  59 46 3F  EOR 3F46,Y
2287  5C          ???
2288  0A        ASL A
2289  0E F9 07  ASL 07F9
228C  08        PHP 
228D  62          ???
228E  8F          ???
228F  BA        TSX 
2290  96 D5     STX D5,Y
2292  23          ???
2293  AD 25 E8  LDA E825
2296  4B          ???
2297  01 F6     ORA (F6,X)
2299  22          ???
229A  88        DEY 
229B  87          ???
229C  1C          ???
229D  11 D5     ORA (D5),Y
229F  01 40     ORA (40,X)
22A1  60        RTS 
22A2  DF          ???
22A3  15 3C     ORA 3C,X
22A5  4E 16 6B  LSR 6B16
22A8  60        RTS 
22A9  53          ???
22AA  80          ???
22AB  A2 05     LDX #05
22AD  4C 54 FF  JMP FF54
22B0  A0 7F     LDY #7F
22B2  1F          ???
22B3  D1 31     CMP (31),Y
22B5  63          ???
22B6  21 3E     AND (3E,X)
22B8  31 93     AND (93),Y
22BA  31 A1     AND (A1),Y
22BC  85 14     STA 14
22BE  9E          ???
22BF  42          ???
22C0  AD 69 47  LDA 4769
22C3  C9 52     CMP #52
22C5  67          ???
22C6  EF          ???
22C7  FA          ???
22C8  56 F5     LSR F5,X
22CA  56 BC     LSR BC,X
22CC  1A          ???
22CD  F8        SED 
22CE  CD 84 30  CMP 3084
22D1  9C          ???
22D2  24 54     BIT 54
22D4  86 31     STX 31
22D6  BD DA 49  LDA 49DA,X
22D9  D7          ???
22DA  DE 35 56  DEC 5635,X
22DD  A0 05     LDY #05
22DF  20 E8 F7  JSR F7E8
22E2  FC          ???
22E3  96 EB     STX EB,Y
22E5  13          ???
22E6  8D 74 C3  STA C374
22E9  43          ???
22EA  8B          ???
22EB  C6 19     DEC 19
22ED  43          ???
22EE  96 C1     STX C1,Y
22F0  A6 0B     LDX 0B
22F2  A4 0C     LDY 0C
22F4  F8        SED 
22F5  6A        ROR A
22F6  C1 9A     CMP (9A,X)
22F8  81 14     STA (14,X)
22FA  A5 0E     LDA 0E
22FC  D9 B2 A3  CMP A3B2,Y
22FF  61 8A     ADC (8A,X)
2301  D6 82     DEC 82,X
2303  5C          ???
2304  CA        DEX 
2305  99 C8 4B  STA 4BC8,Y
2308  95 0C     STA 0C,X
230A  6A        ROR A
230B  E3          ???
230C  0D D5 F0  ORA F0D5
230F  E6 0E     INC 0E
2311  73          ???
2312  60        RTS 
2313  6A        ROR A
2314  D6 4C     DEC 4C,X
2316  BD CC BC  LDA BCCC,X
2319  3A          ???
231A  CF          ???
231B  62          ???
231C  E3          ???
231D  13          ???
231E  98        TYA 
231F  AD D2 09  LDA 09D2
2322  06 6D     ASL 6D
2324  4F          ???
2325  02          ???
2326  7E 05 A0  ROR A005,X
2329  E1 8D     SBC (8D,X)
232B  B1 04     LDA (04),Y
232D  49 FD     EOR #FD
232F  99 2F AD  STA AD2F,Y
2332  F3          ???
2333  43          ???
2334  0C          ???
2335  2C 0D B5  BIT B50D
2338  0E 6A 19  ASL 196A
233B  EA        NOP 
233C  18        CLC 
233D  31 4C     AND (4C),Y
233F  60        RTS 
2340  0A        ASL A
2341  C4 15     CPY 15
2343  41 0A     EOR (0A,X)
2345  F4          ???
2346  4C 94 C7  JMP C794
2349  C1 CB     CMP (CB,X)
234B  5E C0 05  LSR 05C0,X
234E  D0 F3     BNE 2343
2350  0F          ???
2351  4B          ???
2352  79 49 49  ADC 4949,Y
2355  B7          ???
2356  1C          ???
2357  20 47 F4  JSR F447
235A  F2          ???
235B  61 C1     ADC (C1,X)
235D  C8        INY 
235E  53          ???
235F  8F          ???
2360  09 7B     ORA #7B
2362  15 23     ORA 23,X
2364  32          ???
2365  05 B6     ORA B6
2367  CF          ???
2368  BD 81 63  LDA 6381,X
236B  07          ???
236C  E3          ???
236D  18        CLC 
236E  40        RTI 
236F  3C          ???
2370  CF          ???
2371  66 89     ROR 89
2373  8C C2 EE  STY EEC2
2376  CE AB 24  DEC 24AB
2379  08        PHP 
237A  88        DEY 
237B  43          ???
237C  C9 95     CMP #95
237E  D0 02     BNE 2382
2380  DE 62 56  DEC 5662,X
2383  4C 5B A5  JMP A55B
2386  53          ???
2387  C5 38     CMP 38
2389  F0 A0     BEQ 232B
238B  2C 6E 49  BIT 496E
238E  A6 C4     LDX C4
2390  A8        TAY 
2391  99 A7 D4  STA D4A7,Y
2394  69 47     ADC #47
2396  FA          ???
2397  31 21     AND (21),Y
2399  A6 6A     LDX 6A
239B  8E 97 4C  STX 4C97
239E  18        CLC 
239F  F4          ???
23A0  9E          ???
23A1  39 E7 73  AND 73E7,Y
23A4  C8        INY 
23A5  E7          ???
23A6  9D CF 0E  STA 0ECF,X
23A9  2C 47 0A  BIT 0A47
23AC  1E E7 8D  ASL 8DE7,X
23AF  6C FC FF  JMP (FFFC)
23B2  7B          ???
23B3  6C 58 25  JMP (2558)
23B6  22          ???
23B7  FB          ???
23B8  78        SEI 
23B9  A2 B7     LDX #B7
23BB  9C          ???
23BC  CA        DEX 
23BD  30 EA     BMI 23A9
23BF  3B          ???
23C0  CC 4A A3  CPY A34A
23C3  54          ???
23C4  90 02     BCC 23C8
23C6  B3          ???
23C7  6E 19 C4  ROR C419
23CA  C6 30     DEC 30
23CC  48        PHA 
23CD  29 32     AND #32
23CF  C9 05     CMP #05
23D1  F0 F7     BEQ 23CA
23D3  F7          ???
23D4  F6 CA     INC CA,X
23D6  D0 D9     BNE 23B1
23D8  A0 1F     LDY #1F
23DA  7A          ???
23DB  D4          ???
23DC  0B          ???
23DD  35 75     AND 75,X
23DF  85 0A     STA 0A
23E1  19 07 E9  ORA E907,Y
23E4  0C          ???
23E5  69 30     ADC #30
23E7  32          ???
23E8  0B          ???
23E9  7A          ???
23EA  78        SEI 
23EB  47          ???
23EC  C8        INY 
23ED  21 5A     AND (5A,X)
23EF  EB          ???
23F0  4B          ???
23F1  93          ???
23F2  B9 AD 04  LDA 04AD,Y
23F5  D7          ???
23F6  51 6B     EOR (6B),Y
23F8  23          ???
23F9  2A        ROL A
23FA  56 B1     LSR B1,X
23FC  E0 85     CPX #85
23FE  FB          ???
23FF  58        CLI 
2400  2A        ROL A
2401  02          ???
2402  30 FC     BMI 2400
2404  D7          ???
2405  78        SEI 
2406  A5 0A     LDA 0A
2408  FB          ???
2409  F2          ???
240A  AD 46 29  LDA 2946
240D  F7          ???
240E  BA        TSX 
240F  1C          ???
2410  4B          ???
2411  50 06     BVC 2419
2413  C7          ???
2414  E1 EA     SBC (EA,X)
2416  86 5D     STX 5D
2418  81 12     STA (12,X)
241A  78        SEI 
241B  30 DB     BMI 23F8
241D  85 B9     STA B9
241F  31 20     AND (20),Y
2421  0A        ASL A
2422  EF          ???
2423  9A        TXS 
2424  10 C9     BPL 23EF
2426  55 65     EOR 65,X
2428  EB          ???
2429  BE 48 10  LDX 1048,Y
242C  32          ???
242D  F4          ???
242E  50 FE     BVC 242E
2430  B8        CLV 
2431  AD 01 1C  LDA 1C01
2434  91 FE     STA (FE),Y
2436  98        TYA 
2437  34          ???
2438  F5 E6     SBC E6,X
243A  D7          ???
243B  6B          ???
243C  38        SEC 
243D  F0 E0     BEQ 241F
243F  AD C8 D0  LDA D0C8
2442  04          ???
2443  A0 BE     LDY #BE
2445  1F          ???
2446  C6 31     DEC 31
2448  DA          ???
2449  12          ???
244A  0A        ASL A
244B  72          ???
244C  16 0F     ASL 0F,X
244E  D1 6A     CMP (6A),Y
2450  31 28     AND (28),Y
2452  85 D2     STA D2
2454  88        DEY 
2455  CA        DEX 
2456  D8        CLD 
2457  02          ???
2458  77          ???
2459  54          ???
245A  49 46     EOR #46
245C  A4 20     LDY 20
245E  82          ???
245F  86 A5     STX A5
2461  55 4B     EOR 4B,X
2463  C9 10     CMP #10
2465  B0 1F     BCS 2486
2467  BA        TSX 
2468  43          ???
2469  C8        INY 
246A  E6 8B     INC 8B
246C  D1 30     CMP (30),Y
246E  29 29     AND #29
2470  07          ???
2471  85 FA     STA FA
2473  E5 11     SBC 11
2475  0A        ASL A
2476  26 1C     ROL 1C
2478  A5 52     LDA 52
247A  9D BE 0C  STA 0CBE,X
247D  E8        INX 
247E  D0 92     BNE 2412
2480  E1 48     SBC (48,X)
2482  29 1F     AND #1F
2484  AA        TAX 
2485  C8        INY 
2486  B1 DF     LDA (DF),Y
2488  30 6A     BMI 24F4
248A  DD 18 A8  CMP A818,X
248D  F7          ???
248E  19 A3 D4  ORA D4A3,Y
2491  3F          ???
2492  EB          ???
2493  90 CC     BCC 2461
2495  00        BRK 
2496  26 61     ROL 61
2498  D1 98     CMP (98),Y
249A  40        RTI 
249B  4C C3 A3  JMP A3C3
249E  30 80     BMI 2420
24A0  98        TYA 
24A1  86 03     STX 03
24A3  4F          ???
24A4  53          ???
24A5  BE A4 02  LDX 02A4,Y
24A8  5D AE 74  EOR 74AE,X
24AB  88        DEY 
24AC  10 CD     BPL 247B
24AE  AA        TAX 
24AF  F0 FF     BEQ 24B0
24B1  EE 5E 8F  INC 8F5E
24B4  4C 16 06  JMP 0616
24B7  7C          ???
24B8  8E 00 61  STX 6100
24BB  73          ???
24BC  53          ???
24BD  0E 03 BD  ASL BD03
24C0  A0 4F     LDY #4F
24C2  D4          ???
24C3  01 02     ORA (02,X)
24C5  1D C0 EF  ORA EFC0,X
24C8  F8        SED 
24C9  85 56     STA 56
24CB  E7          ???
24CC  60        RTS 
24CD  B8        CLV 
24CE  A2 25     LDX #25
24D0  74          ???
24D1  AE 0C B6  LDX B60C
24D4  D8        CLD 
24D5  E8        INX 
24D6  8A        TXA 
24D7  A0 FF     LDY #FF
24D9  20 5F 3E  JSR 3E5F
24DC  EE A9 8C  INC 8CA9
24DF  DB          ???
24E0  45 73     EOR 73
24E2  60        RTS 
24E3  48        PHA 
24E4  60        RTS 
24E5  4A        LSR A
24E6  8B          ???
24E7  44          ???
24E8  2F          ???
24E9  65 07     ADC 07
24EB  68        PLA 
24EC  7B          ???
24ED  0F          ???
24EE  AA        TAX 
24EF  BD 7B 05  LDA 057B,X
24F2  DF          ???
24F3  A2 8E     LDX #8E
24F5  CB          ???
24F6  AE 13 D0  LDX D013
24F9  49 FB     EOR #FB
24FB  99 A9 C1  STA C1A9,Y
24FE  EA        NOP 
24FF  2A        ROL A
2500  92          ???
2501  25 0A     AND 0A
2503  3A          ???
2504  DE 89 D9  DEC D989,X
2507  09 20     ORA #20
2509  23          ???
250A  D0 94     BNE 24A0
250C  CA        DEX 
250D  29 DF     AND #DF
250F  8D 4E 18  STA 184E
2512  60        RTS 
2513  DB          ???
2514  03          ???
2515  05 0E     ORA 0E
2517  0F          ???
2518  07          ???
2519  5E 0A 0B  LSR 0B0A,X
251C  03          ???
251D  AE 22 0D  LDX 0D22
2520  BF          ???
2521  AA        TAX 
2522  00        BRK 
2523  09 01     ORA #01
2525  F5 06     SBC 06,X
2527  0C          ???
2528  04          ???
2529  FF          ???
252A  7A          ???
252B  63          ???
252C  DE D0 07  DEC 07D0,X
252F  F1 1C     SBC (1C),Y
2531  F4          ???
2532  5B          ???
2533  6F          ???
2534  27          ???
2535  AD AA 02  LDA 02AA
2538  AE 74 D6  LDX D674
253B  34          ???
253C  D4          ???
253D  3C          ???
253E  18        CLC 
253F  35 4C     AND 4C,X
2541  8E 05 E0  STX E005
2544  4C 00 02  JMP 0200
2547  31 BF     AND (BF),Y
2549  E3          ???
254A  5F          ???
254B  1B          ???
254C  E5 5F     SBC 5F
254E  34          ???
254F  20 6C 21  JSR 216C
2552  64          ???
2553  B5 FA     LDA FA,X
2555  5A          ???
2556  BF          ???
2557  85 9A     STA 9A
2559  6D 08 5A  ADC 5A08
255C  16 D2     ASL D2,X
255E  14          ???
255F  5A          ???
2560  18        CLC 
2561  86 80     STX 80
2563  96 91     STX 91,Y
2565  02          ???
2566  B6 EA     LDX EA,Y
2568  AD 28 5E  LDA 5E28
256B  DC          ???
256C  5B          ???
256D  CE BC A4  DEC A4BC
2570  6C 22 D5  JMP (D522)
2573  A4 63     LDY 63
2575  23          ???
2576  89          ???
2577  8D 24 5E  STA 5E24
257A  E2          ???
257B  A9 0C     LDA #0C
257D  D3          ???
257E  D8        CLD 
257F  89          ???
2580  D9 89 DA  CMP DA89,Y
2583  89          ???
2584  9D DB F5  STA F5DB,X
2587  6A        ROR A
2588  F1 69     SBC (69),Y
258A  6D 46 97  ADC 9746
258D  04          ???
258E  10 6C     BPL 25FC
2590  6B          ???
2591  18        CLC 
2592  9C          ???
2593  C1 AD     CMP (AD,X)
2595  AC BD 4B  LDY 4BBD
2598  65 BB     ADC BB
259A  06 52     ASL 52
259C  9C          ???
259D  E8        INX 
259E  4D D0 F5  EOR F5D0
25A1  CA        DEX 
25A2  8F          ???
25A3  51 DA     EOR (DA),Y
25A5  E4 FF     CPX FF
25A7  F0 FB     BEQ 25A4
25A9  F2          ???
25AA  1E 52 4C  ASL 4C52,X
25AD  6F          ???
25AE  50 DF     BVC 258F
25B0  59 F4 C5  EOR C5F4,Y
25B3  DB          ???
25B4  EC 07 00  CPX 0007
25B7  05 DD     ORA DD
25B9  9F          ???
25BA  CC 34 39  CPY 3934
25BD  28        PLP 
25BE  E1 22     SBC (22,X)
25C0  02          ???
25C1  CB          ???
25C2  A5 C4     LDA C4
25C4  E1 54     SBC (54,X)
25C6  0C          ???
25C7  47          ???
25C8  BA        TSX 
25C9  7E B0 3A  ROR 3AB0,X
25CC  43          ???
25CD  4B          ???
25CE  B3          ???
25CF  A4 20     LDY 20
25D1  33          ???
25D2  98        TYA 
25D3  44          ???
25D4  35 59     AND 59,X
25D6  52          ???
25D7  66 6A     ROR 6A
25D9  50 58     BVC 2633
25DB  71 56     ADC (56),Y
25DD  54          ???
25DE  24 57     BIT 57
25E0  12          ???
25E1  DD 79 42  CMP 4279,X
25E4  65 74     ADC 74
25E6  D7          ???
25E7  4B          ???
25E8  49 46     EOR #46
25EA  29 D3     AND #D3
25EC  4D 4F DE  EOR DE4F
25EF  86 59     STX 59
25F1  56 B2     LSR B2,X
25F3  41 A9     EOR (A9,X)
25F5  D4          ???
25F6  8B          ???
25F7  A2 4D     LDX #4D
25F9  82          ???
25FA  53          ???
25FB  48        PHA 
25FC  B5 43     LDA 43,X
25FE  50 67     BVC 2667
2600  26 12     ROL 12
2602  46 52     LSR 52
2604  AD 42 AD  LDA AD42
2607  CE 49 96  DEC 9649
260A  4D 2D 4E  EOR 4E2D
260D  6F          ???
260E  4C 1F 4B  JMP 4B1F
2611  46 45     LSR 45
2613  50 55     BVC 266A
2615  4C 4F D0  JMP D04F
2618  16 42     ASL 42,X
261A  55 4F     EOR 4F,X
261C  8E 8D C5  STX C58D
261F  57          ???
2620  B3          ???
2621  CB          ???
2622  43          ???
2623  48        PHA 
2624  B0 45     BCS 266B
2626  D2          ???
2627  18        CLC 
2628  4E D5 B1  LSR B1D5
262B  46 6D     LSR 6D
262D  33          ???
262E  92          ???
262F  D4          ???
2630  B6 29     LDX 29,Y
2632  A1 54     LDA (54,X)
2634  DC          ???
2635  47          ???
2636  D9 4D C5  CMP C54D,Y
2639  DE 0C D5  DEC D50C,X
263C  CE 81 43  DEC 4381
263F  AD 4D C3  LDA C34D
2642  B3          ???
2643  41 55     EOR (55,X)
2645  44          ???
2646  8D D2 0D  STA 0DD2
2649  CC 55 52  CPY 5255
264C  4E D7 2A  LSR 2AD7
264F  4F          ???
2650  37          ???
2651  53          ???
2652  36 4C     ROL 4C,X
2654  45 43     EOR 43
2656  54          ???
2657  2E 5F 00  ROL 005F
265A  9B          ???
265B  C1 40     CMP (40,X)
265D  9A        TXS 
265E  92          ???
265F  31 53     AND (53),Y
2661  C4 B1     CPY B1
2663  B3          ???
2664  14          ???
2665  C6 9B     DEC 9B
2667  D4          ???
2668  C8        INY 
2669  C5 57     CMP 57
266B  C3          ???
266C  D2          ???
266D  EB          ???
266E  CF          ???
266F  D7          ???
2670  CE 20 2B  DEC 2B20
2673  39 FF 74  AND 74FF,Y
2676  8D 6C 50  STA 506C
2679  6C FC 6C  JMP (6CFC)
267C  FD 6C 6E  SBC 6E6C,X
267F  C2          ???
2680  CC E4 FA  CPY FAE4
2683  1E B1 05  ASL 05B1,X
2686  85 5A     STA 5A
2688  CF          ???
2689  00        BRK 
268A  B1 FC     LDA (FC),Y
268C  F0 0B     BEQ 2699
268E  C9 BF     CMP #BF
2690  C0 B0     CPY #B0
2692  44          ???
2693  ED EC 0F  SBC 0FEC
2696  F1 7A     SBC (7A),Y
2698  F9 C6 7C  SBC 7CC6,Y
269B  C6 04     DEC 04
269D  86 FE     STX FE
269F  84 FF     STY FF
26A1  A2 00     LDX #00
26A3  A0 FF     LDY #FF
26A5  02          ???
26A6  FE 58 D0  INC D058,X
26A9  A7          ???
26AA  B2          ???
26AB  32          ???
26AC  7B          ???
26AD  7D 8D D9  ADC D98D,X
26B0  3C          ???
26B1  B9 80 50  LDA 5080,Y
26B4  6F          ???
26B5  93          ???
26B6  0A        ASL A
26B7  76 5E     ROR 5E,X
26B9  9D F3 51  STA 51F3,X
26BC  88        DEY 
26BD  10 E1     BPL 26A0
26BF  7E 19 01  ROR 0119,X
26C2  D0 99     BNE 265D
26C4  D7          ???
26C5  86 A5     STX A5
26C7  D4          ???
26C8  DC          ???
26C9  D7          ???
26CA  FF          ???
26CB  60        RTS 
26CC  49 77     EOR #77
26CE  80          ???
26CF  45 FB     EOR FB
26D1  91 FB     STA (FB),Y
26D3  C8        INY 
26D4  98        TYA 
26D5  DE 65 EF  DEC EF65,X
26D8  48        PHA 
26D9  FD A3 FC  SBC FCA3,X
26DC  20 BB 12  JSR 12BB
26DF  52          ???
26E0  FF          ???
26E1  4C A1 51  JMP 51A1
26E4  A5 7A     LDA 7A
26E6  18        CLC 
26E7  69 28     ADC #28
26E9  90 6F     BCC 275A
26EB  8D FF 85  STA 85FF
26EE  FE 3F A6  INC A63F,X
26F1  69 6A     ADC #6A
26F3  11 DA     ORA (DA),Y
26F5  08        PHP 
26F6  E8        INX 
26F7  E4 F9     CPX F9
26F9  DE 54 E6  DEC E654,X
26FC  F4          ???
26FD  91 89     STA (89),Y
26FF  07          ???
2700  69 F0     ADC #F0
2702  02          ???
2703  53          ???
2704  C6 86     DEC 86
2706  C9 5D     CMP #5D
2708  0D D0 0C  ORA 0CD0
270B  A5 FA     LDA FA
270D  0A        ASL A
270E  A8        TAY 
270F  FF          ???
2710  49 54     EOR #54
2712  B9 93 2A  LDA 2A93,Y
2715  48        PHA 
2716  60        RTS 
2717  5B          ???
2718  BC 69 B2  LDY B269,X
271B  77          ???
271C  85 CB     STA CB
271E  93          ???
271F  2C A1 B2  BIT B2A1
2722  AF          ???
2723  BD CB CB  LDA CBCB,X
2726  2C E2 A5  BIT A5E2
2729  0B          ???
272A  A4 7F     LDY 7F
272C  0B          ???
272D  9A        TXS 
272E  88        DEY 
272F  35 06     AND 06,X
2731  E2          ???
2732  11 81     ORA (81),Y
2734  71 AD     ADC (AD),Y
2736  83          ???
2737  D4          ???
2738  CE F6 1C  DEC 1CF6
273B  36 DC     ROL DC,X
273D  50 C3     BVC 2702
273F  2E 30 00  ROL 0030
2742  7A          ???
2743  01 9E     ORA (9E,X)
2745  D2          ???
2746  D7          ???
2747  18        CLC 
2748  A5 3D     LDA 3D
274A  C6 4F     DEC 4F
274C  FB          ???
274D  35 6D     AND 6D,X
274F  CA        DEX 
2750  00        BRK 
2751  DD AD 4F  CMP 4FAD,X
2754  6A        ROR A
2755  36 DE     ROL DE,X
2757  20 DA 52  JSR 52DA
275A  F0 03     BEQ 275F
275C  DF          ???
275D  2A        ROL A
275E  2C A9 00  BIT 00A9
2761  B7          ???
2762  8D 1F 33  STA 331F
2765  E9 BD     SBC #BD
2767  B4 49     LDY 49,X
2769  01 9D     ORA (9D,X)
276B  83          ???
276C  50 9F     BVC 270D
276E  28        PLP 
276F  68        PLA 
2770  60        RTS 
2771  5E 62 40  LSR 4062,X
2774  8F          ???
2775  73          ???
2776  51 16     EOR (16),Y
2778  62          ???
2779  20 40 45  JSR 4540
277C  9A        TXS 
277D  8F          ???
277E  73          ???
277F  35 16     AND 16,X
2781  62          ???
2782  60        RTS 
2783  45 0D     EOR 0D
2785  4A        LSR A
2786  97          ???
2787  51 66     EOR (66),Y
2789  24 00     BIT 00
278B  20 E6 52  JSR 52E6
278E  E8        INX 
278F  A9 00     LDA #00
2791  85 FC     STA FC
2793  85 FB     STA FB
2795  E0 01     CPX #01
2797  90 28     BCC 27C1
2799  A5 FD     LDA FD
279B  4A        LSR A
279C  D0 18     BNE 27B6
279E  AD 23 01  LDA 0123
27A1  D0 0A     BNE 27AD
27A3  C6 01     DEC 01
27A5  8E E7 DB  STX DBE7
27A8  E6 01     INC 01
27AA  CE 24 01  DEC 0124
27AD  CE 23 01  DEC 0123
27B0  AD 8B 27  LDA 278B
27B3  90 1B     BCC 27D0
27B5  6A        ROR A
27B6  26 FC     ROL FC
27B8  26 FB     ROL FB
27BA  CA        DEX 
27BB  D0 DE     BNE 279B
27BD  85 FD     STA FD
27BF  A5 FC     LDA FC
27C1  60        RTS 
27C2  C6 01     DEC 01
27C4  58        CLI 
27C5  4C 00 19  JMP 1900
27C8  CA        DEX 
27C9  C6 FF     DEC FF
27CB  C6 AF     DEC AF
27CD  88        DEY 
27CE  B1 AE     LDA (AE),Y
27D0  91 FE     STA (FE),Y
27D2  98        TYA 
27D3  D0 F8     BNE 27CD
27D5  8A        TXA 
27D6  D0 F0     BNE 27C8
27D8  20 00 01  JSR 0100
27DB  F0 0A     BEQ 27E7
27DD  A5 FE     LDA FE
27DF  D0 02     BNE 27E3
27E1  C6 FF     DEC FF
27E3  C6 FE     DEC FE
27E5  90 B7     BCC 279E
27E7  C8        INY 
27E8  20 00 01  JSR 0100
27EB  F0 FA     BEQ 27E7
27ED  C0 11     CPY #11
27EF  B0 D1     BCS 27C2
27F1  BE 33 03  LDX 0333,Y
27F4  20 01 01  JSR 0101
27F7  79 67 03  ADC 0367,Y
27FA  85 A7     STA A7
27FC  A5 FB     LDA FB
27FE  79 9B 03  ADC 039B,Y
2801  48        PHA 
2802  D0 06     BNE 280A
2804  A4 A7     LDY A7
2806  C0 04     CPY #04
2808  90 02     BCC 280C
280A  A0 03     LDY #03
280C  BE B3 01  LDX 01B3,Y
280F  20 01 01  JSR 0101
2812  79 B6 01  ADC 01B6,Y
2815  A8        TAY 
2816  38        SEC 
2817  A5 FE     LDA FE
2819  E5 A7     SBC A7
281B  85 FE     STA FE
281D  B0 02     BCS 2821
281F  C6 FF     DEC FF
2821  BE 34 03  LDX 0334,Y
2824  20 01 01  JSR 0101
2827  79 68 03  ADC 0368,Y
282A  90 03     BCC 282F
282C  E6 FB     INC FB
282E  18        CLC 
282F  65 FE     ADC FE
2831  85 AE     STA AE
2833  A5 FB     LDA FB
2835  79 9C 03  ADC 039C,Y
2838  65 FF     ADC FF
283A  85 AF     STA AF
283C  A4 A7     LDY A7
283E  68        PLA 
283F  AA        TAX 
2840  90 90     BCC 27D2
2842  02          ???
2843  04          ???
2844  04          ???
2845  30 20     BMI 2867
2847  10 E8     BPL 2831
2849  98        TYA 
284A  29 0F     AND #0F
284C  F0 13     BEQ 2861
284E  8A        TXA 
284F  4A        LSR A
2850  A6 FC     LDX FC
2852  2A        ROL A
2853  26 FB     ROL FB
2855  CA        DEX 
2856  10 FA     BPL 2852
2858  79 67 03  ADC 0367,Y
285B  AA        TAX 
285C  A5 FB     LDA FB
285E  79 9B 03  ADC 039B,Y
2861  99 9C 03  STA 039C,Y
2864  8A        TXA 
2865  99 68 03  STA 0368,Y
2868  A2 04     LDX #04
286A  20 01 01  JSR 0101
286D  99 34 03  STA 0334,Y
2870  C8        INY 
2871  C0 34     CPY #34
2873  D0 D3     BNE 2848
2875  A0 00     LDY #00
2877  8C E7 07  STY 07E7
