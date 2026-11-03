// Minimal 6502 disassembler for inspecting the 1541 DOS ROM.
const fs = require('fs');

const modes = {
  imp: 1, acc: 1, imm: 2, zp: 2, zpx: 2, zpy: 2, abs: 3, abx: 3, aby: 3,
  ind: 3, izx: 2, izy: 2, rel: 2
};

const table = {};
function op(code, name, mode) { table[code] = [name, mode]; }

op(0x00,'BRK','imp'); op(0x01,'ORA','izx'); op(0x05,'ORA','zp'); op(0x06,'ASL','zp');
op(0x08,'PHP','imp'); op(0x09,'ORA','imm'); op(0x0A,'ASL','acc'); op(0x0D,'ORA','abs');
op(0x0E,'ASL','abs'); op(0x10,'BPL','rel'); op(0x11,'ORA','izy'); op(0x15,'ORA','zpx');
op(0x16,'ASL','zpx'); op(0x18,'CLC','imp'); op(0x19,'ORA','aby'); op(0x1D,'ORA','abx');
op(0x1E,'ASL','abx'); op(0x20,'JSR','abs'); op(0x21,'AND','izx'); op(0x24,'BIT','zp');
op(0x25,'AND','zp'); op(0x26,'ROL','zp'); op(0x28,'PLP','imp'); op(0x29,'AND','imm');
op(0x2A,'ROL','acc'); op(0x2C,'BIT','abs'); op(0x2D,'AND','abs'); op(0x2E,'ROL','abs');
op(0x30,'BMI','rel'); op(0x31,'AND','izy'); op(0x35,'AND','zpx'); op(0x36,'ROL','zpx');
op(0x38,'SEC','imp'); op(0x39,'AND','aby'); op(0x3D,'AND','abx'); op(0x3E,'ROL','abx');
op(0x40,'RTI','imp'); op(0x41,'EOR','izx'); op(0x45,'EOR','zp'); op(0x46,'LSR','zp');
op(0x48,'PHA','imp'); op(0x49,'EOR','imm'); op(0x4A,'LSR','acc'); op(0x4C,'JMP','abs');
op(0x4D,'EOR','abs'); op(0x4E,'LSR','abs'); op(0x50,'BVC','rel'); op(0x51,'EOR','izy');
op(0x55,'EOR','zpx'); op(0x56,'LSR','zpx'); op(0x58,'CLI','imp'); op(0x59,'EOR','aby');
op(0x5D,'EOR','abx'); op(0x5E,'LSR','abx'); op(0x60,'RTS','imp'); op(0x61,'ADC','izx');
op(0x65,'ADC','zp'); op(0x66,'ROR','zp'); op(0x68,'PLA','imp'); op(0x69,'ADC','imm');
op(0x6A,'ROR','acc'); op(0x6C,'JMP','ind'); op(0x6D,'ADC','abs'); op(0x6E,'ROR','abs');
op(0x70,'BVS','rel'); op(0x71,'ADC','izy'); op(0x75,'ADC','zpx'); op(0x76,'ROR','zpx');
op(0x78,'SEI','imp'); op(0x79,'ADC','aby'); op(0x7D,'ADC','abx'); op(0x7E,'ROR','abx');
op(0x81,'STA','izx'); op(0x84,'STY','zp'); op(0x85,'STA','zp'); op(0x86,'STX','zp');
op(0x88,'DEY','imp'); op(0x8A,'TXA','imp'); op(0x8C,'STY','abs'); op(0x8D,'STA','abs');
op(0x8E,'STX','abs'); op(0x90,'BCC','rel'); op(0x91,'STA','izy'); op(0x94,'STY','zpx');
op(0x95,'STA','zpx'); op(0x96,'STX','zpy'); op(0x98,'TYA','imp'); op(0x99,'STA','aby');
op(0x9A,'TXS','imp'); op(0x9D,'STA','abx'); op(0xA0,'LDY','imm'); op(0xA1,'LDA','izx');
op(0xA2,'LDX','imm'); op(0xA4,'LDY','zp'); op(0xA5,'LDA','zp'); op(0xA6,'LDX','zp');
op(0xA8,'TAY','imp'); op(0xA9,'LDA','imm'); op(0xAA,'TAX','imp'); op(0xAC,'LDY','abs');
op(0xAD,'LDA','abs'); op(0xAE,'LDX','abs'); op(0xB0,'BCS','rel'); op(0xB1,'LDA','izy');
op(0xB4,'LDY','zpx'); op(0xB5,'LDA','zpx'); op(0xB6,'LDX','zpy'); op(0xB8,'CLV','imp');
op(0xB9,'LDA','aby'); op(0xBA,'TSX','imp'); op(0xBC,'LDY','abx'); op(0xBD,'LDA','abx');
op(0xBE,'LDX','aby'); op(0xC0,'CPY','imm'); op(0xC1,'CMP','izx'); op(0xC4,'CPY','zp');
op(0xC5,'CMP','zp'); op(0xC6,'DEC','zp'); op(0xC8,'INY','imp'); op(0xC9,'CMP','imm');
op(0xCA,'DEX','imp'); op(0xCC,'CPY','abs'); op(0xCD,'CMP','abs'); op(0xCE,'DEC','abs');
op(0xD0,'BNE','rel'); op(0xD1,'CMP','izy'); op(0xD5,'CMP','zpx'); op(0xD6,'DEC','zpx');
op(0xD8,'CLD','imp'); op(0xD9,'CMP','aby'); op(0xDD,'CMP','abx'); op(0xDE,'DEC','abx');
op(0xE0,'CPX','imm'); op(0xE1,'SBC','izx'); op(0xE4,'CPX','zp'); op(0xE5,'SBC','zp');
op(0xE6,'INC','zp'); op(0xE8,'INX','imp'); op(0xE9,'SBC','imm'); op(0xEA,'NOP','imp');
op(0xEC,'CPX','abs'); op(0xED,'SBC','abs'); op(0xEE,'INC','abs'); op(0xF0,'BEQ','rel');
op(0xF1,'SBC','izy'); op(0xF5,'SBC','zpx'); op(0xF6,'INC','zpx'); op(0xF8,'SED','imp');
op(0xF9,'SBC','aby'); op(0xFD,'SBC','abx'); op(0xFE,'INC','abx');

const rom = fs.readFileSync(process.argv[2] || 'C64Emulator/Roms/dos1541.rom');
const start = parseInt(process.argv[3] || 'EC00', 16);
const end = parseInt(process.argv[4] || 'EC80', 16);
const org = parseInt(process.argv[5] || 'C000', 16);

function byteAt(addr) { return rom[addr - org]; }
function wordAt(addr) { return byteAt(addr) | (byteAt(addr + 1) << 8); }

for (let addr = start; addr < end; ) {
  const code = byteAt(addr);
  const entry = table[code];

  if (!entry) {
    console.log(addr.toString(16).toUpperCase().padStart(4, '0') + '  ' +
      code.toString(16).toUpperCase().padStart(2, '0') + '          ???');
    addr += 1;
    continue;
  }

  const [name, mode] = entry;
  const len = modes[mode];
  const raw = [];
  for (let i = 0; i < len; i++) raw.push(byteAt(addr + i).toString(16).toUpperCase().padStart(2, '0'));

  let operand = '';
  const lo = byteAt(addr + 1);
  const hi = byteAt(addr + 2);

  switch (mode) {
    case 'imp': break;
    case 'acc': operand = 'A'; break;
    case 'imm': operand = '#' + lo.toString(16).toUpperCase().padStart(2, '0'); break;
    case 'zp': operand = lo.toString(16).toUpperCase().padStart(2, '0'); break;
    case 'zpx': operand = lo.toString(16).toUpperCase().padStart(2, '0') + ',X'; break;
    case 'zpy': operand = lo.toString(16).toUpperCase().padStart(2, '0') + ',Y'; break;
    case 'abs': operand = wordAt(addr + 1).toString(16).toUpperCase().padStart(4, '0'); break;
    case 'abx': operand = wordAt(addr + 1).toString(16).toUpperCase().padStart(4, '0') + ',X'; break;
    case 'aby': operand = wordAt(addr + 1).toString(16).toUpperCase().padStart(4, '0') + ',Y'; break;
    case 'ind': operand = '(' + wordAt(addr + 1).toString(16).toUpperCase().padStart(4, '0') + ')'; break;
    case 'izx': operand = '(' + lo.toString(16).toUpperCase().padStart(2, '0') + ',X)'; break;
    case 'izy': operand = '(' + lo.toString(16).toUpperCase().padStart(2, '0') + '),Y'; break;
    case 'rel': {
      let target = addr + 2 + (lo < 0x80 ? lo : lo - 256);
      operand = target.toString(16).toUpperCase().padStart(4, '0');
      break;
    }
  }

  console.log(
    addr.toString(16).toUpperCase().padStart(4, '0') + '  ' +
    raw.join(' ').padEnd(9) + ' ' + name + ' ' + operand);
  addr += len;
}
