#pragma once

#ifdef GAMEBOYREF_EXPORTS
#define GAMEBOY_API __declspec(dllexport) 
#else
#define GAMEBOY_API __declspec(dllimport) 
#endif

#include <cstdlib>
#include <cinttypes>

namespace gameboy {
	class CPURegisters;
	class Memory;
}

namespace gameboy {
	class GAMEBOY_API Core {
	public:
		explicit Core(uint8_t *initMem);
		virtual ~Core();
		void emulateCycle();
		void handleCB();
		void xx();
		void CBxx();

	public:
		CPURegisters *registers;
		Memory *memory;

	private:
		bool conditional;
		unsigned int clock;

		typedef void (Core::*opCode) ();
		static const opCode opCodes[];
		static const opCode opCodesCB[];
		static const uint8_t opCodeCycles[];
		static const uint8_t opCodeCondCycles[];
		static const uint8_t opCodeCBCycles[];

	private:
		void LDrnA();
		void LDrnB();
		void LDrnC();
		void LDrnD();
		void LDrnE();
		void LDrnH();
		void LDrnL();
		void LDrrAA();
		void LDrrAB();
		void LDrrAC();
		void LDrrAD();
		void LDrrAE();
		void LDrrAH();
		void LDrrAL();
		void LDrrBA();
		void LDrrBB();
		void LDrrBC();
		void LDrrBD();
		void LDrrBE();
		void LDrrBH();
		void LDrrBL();
		void LDrrCA();
		void LDrrCB();
		void LDrrCC();
		void LDrrCD();
		void LDrrCE();
		void LDrrCH();
		void LDrrCL();
		void LDrrDA();
		void LDrrDB();
		void LDrrDC();
		void LDrrDD();
		void LDrrDE();
		void LDrrDH();
		void LDrrDL();
		void LDrrEA();
		void LDrrEB();
		void LDrrEC();
		void LDrrED();
		void LDrrEE();
		void LDrrEH();
		void LDrrEL();
		void LDrrHA();
		void LDrrHB();
		void LDrrHC();
		void LDrrHD();
		void LDrrHE();
		void LDrrHH();
		void LDrrHL();
		void LDrrLA();
		void LDrrLB();
		void LDrrLC();
		void LDrrLD();
		void LDrrLE();
		void LDrrLH();
		void LDrrLL();
		void LDrHLMA();
		void LDrHLMB();
		void LDrHLMC();
		void LDrHLMD();
		void LDrHLME();
		void LDrHLMH();
		void LDrHLML();
		void LDABCM();
		void LDADEM();
		void LDAmm();
		void LDHLMrA();
		void LDHLMrB();
		void LDHLMrC();
		void LDHLMrD();
		void LDHLMrE();
		void LDHLMrH();
		void LDHLMrL();
		void LDHLmn();
		void LDBCMA();
		void LDDEMA();
		void LDnnA();
		void LDDHLA();
		void LDDAHL();
		void LDIHLA();
		void LDIAHL();
		void LDIOnA();
		void LDAIOn();
		void LDIOCA();
		void LDAIOC();
		void LDBCnn();
		void LDDEnn();
		void LDHLnn();
		void LDSPnn();
		void LDnnSP();
		void LDHLSPn();
		void LDSPHL();
		void ADDrA();
		void ADDrB();
		void ADDrC();
		void ADDrD();
		void ADDrE();
		void ADDrH();
		void ADDrL();
		void ADDHLM();
		void ADDn();
		void ADCrA();
		void ADCrB();
		void ADCrC();
		void ADCrD();
		void ADCrE();
		void ADCrH();
		void ADCrL();
		void ADCHLM();
		void ADCn();
		void SUBrA();
		void SUBrB();
		void SUBrC();
		void SUBrD();
		void SUBrE();
		void SUBrH();
		void SUBrL();
		void SUBHLM();
		void SUBn();
		void SBCrA();
		void SBCrB();
		void SBCrC();
		void SBCrD();
		void SBCrE();
		void SBCrH();
		void SBCrL();
		void SBCHLM();
		void SBCn();
		void ANDrA();
		void ANDrB();
		void ANDrC();
		void ANDrD();
		void ANDrE();
		void ANDrH();
		void ANDrL();
		void ANDHLM();
		void ANDn();
		void ORrA();
		void ORrB();
		void ORrC();
		void ORrD();
		void ORrE();
		void ORrH();
		void ORrL();
		void ORHLM();
		void ORn();
		void XORrA();
		void XORrB();
		void XORrC();
		void XORrD();
		void XORrE();
		void XORrH();
		void XORrL();
		void XORHLM();
		void XORn();
		void CPrA();
		void CPrB();
		void CPrC();
		void CPrD();
		void CPrE();
		void CPrH();
		void CPrL();
		void CPHLM();
		void CPn();
		void INCrA();
		void INCrB();
		void INCrC();
		void INCrD();
		void INCrE();
		void INCrH();
		void INCrL();
		void INCHLM();
		void DECrA();
		void DECrB();
		void DECrC();
		void DECrD();
		void DECrE();
		void DECrH();
		void DECrL();
		void DECHLM();
		void INCBC();
		void INCDE();
		void INCHL();
		void INCSP();
		void DECBC();
		void DECDE();
		void DECHL();
		void DECSP();
		void ADDHLBC();
		void ADDHLDE();
		void ADDHLHL();
		void ADDHLSP();
		void ADDSPn();
		void JPnn();
		void JPHL();
		void JPNZnn();
		void JPZnn();
		void JPNCnn();
		void JPCnn();
		void JRn();
		void JRNZn();
		void JRZn();
		void JRNCn();
		void JRCn();
		void CALLnn();
		void CALLNZnn();
		void CALLZnn();
		void CALLNCnn();
		void CALLCnn();
		void RET();
		void RETI();
		void RETNZ();
		void RETZ();
		void RETNC();
		void RETC();
		void PUSHAF();
		void PUSHBC();
		void PUSHDE();
		void PUSHHL();
		void POPAF();
		void POPBC();
		void POPDE();
		void POPHL();
		void RST00();
		void RST08();
		void RST10();
		void RST18();
		void RST20();
		void RST28();
		void RST30();
		void RST38();
		void INT40();
		void INT48();
		void INT50();
		void INT58();
		void INT60();
		void NOP();
		void DI();
		void EI();
		void HALT();
		void STOP();
		void SCF();
		void CCF();
		void CPL();
		void DAA();
		void RRCANCB();
		void RRANCB();
		void RLCANCB();
		void RLANCB();
		void SWAPrA();
		void SWAPrB();
		void SWAPrC();
		void SWAPrD();
		void SWAPrE();
		void SWAPrH();
		void SWAPrL();
		void SWAPrHLm();
		void RLCA();
		void RLCB();
		void RLCC();
		void RLCD();
		void RLCE();
		void RLCH();
		void RLCL();
		void RLCHLM();
		void RRCA();
		void RRCB();
		void RRCC();
		void RRCD();
		void RRCE();
		void RRCH();
		void RRCL();
		void RRCHL();
		void RLA();
		void RLB();
		void RLC();
		void RLD();
		void RLE();
		void RLH();
		void RLL();
		void RLHLM();
		void RRA();
		void RRB();
		void RRC();
		void RRD();
		void RRE();
		void RRH();
		void RRL();
		void RRHLM();
		void SRLA();
		void SRLB();
		void SRLC();
		void SRLD();
		void SRLE();
		void SRLH();
		void SRLL();
		void SRLHL();
		void SLAA();
		void SLAB();
		void SLAC();
		void SLAD();
		void SLAE();
		void SLAH();
		void SLAL();
		void SLAHL();
		void SRAA();
		void SRAB();
		void SRAC();
		void SRAD();
		void SRAE();
		void SRAH();
		void SRAL();
		void SRAHL();
		void BIT0A();
		void BIT0B();
		void BIT0C();
		void BIT0D();
		void BIT0E();
		void BIT0H();
		void BIT0L();
		void BIT0HL();
		void BIT1A();
		void BIT1B();
		void BIT1C();
		void BIT1D();
		void BIT1E();
		void BIT1H();
		void BIT1L();
		void BIT1HL();
		void BIT2A();
		void BIT2B();
		void BIT2C();
		void BIT2D();
		void BIT2E();
		void BIT2H();
		void BIT2L();
		void BIT2HL();
		void BIT3A();
		void BIT3B();
		void BIT3C();
		void BIT3D();
		void BIT3E();
		void BIT3H();
		void BIT3L();
		void BIT3HL();
		void BIT4A();
		void BIT4B();
		void BIT4C();
		void BIT4D();
		void BIT4E();
		void BIT4H();
		void BIT4L();
		void BIT4HL();
		void BIT5A();
		void BIT5B();
		void BIT5C();
		void BIT5D();
		void BIT5E();
		void BIT5H();
		void BIT5L();
		void BIT5HL();
		void BIT6A();
		void BIT6B();
		void BIT6C();
		void BIT6D();
		void BIT6E();
		void BIT6H();
		void BIT6L();
		void BIT6HL();
		void BIT7A();
		void BIT7B();
		void BIT7C();
		void BIT7D();
		void BIT7E();
		void BIT7H();
		void BIT7L();
		void BIT7HL();
		void RES0A();
		void RES0B();
		void RES0C();
		void RES0D();
		void RES0E();
		void RES0H();
		void RES0L();
		void RES0HL();
		void RES1A();
		void RES1B();
		void RES1C();
		void RES1D();
		void RES1E();
		void RES1H();
		void RES1L();
		void RES1HL();
		void RES2A();
		void RES2B();
		void RES2C();
		void RES2D();
		void RES2E();
		void RES2H();
		void RES2L();
		void RES2HL();
		void RES3A();
		void RES3B();
		void RES3C();
		void RES3D();
		void RES3E();
		void RES3H();
		void RES3L();
		void RES3HL();
		void RES4A();
		void RES4B();
		void RES4C();
		void RES4D();
		void RES4E();
		void RES4H();
		void RES4L();
		void RES4HL();
		void RES5A();
		void RES5B();
		void RES5C();
		void RES5D();
		void RES5E();
		void RES5H();
		void RES5L();
		void RES5HL();
		void RES6A();
		void RES6B();
		void RES6C();
		void RES6D();
		void RES6E();
		void RES6H();
		void RES6L();
		void RES6HL();
		void RES7A();
		void RES7B();
		void RES7C();
		void RES7D();
		void RES7E();
		void RES7H();
		void RES7L();
		void RES7HL();
		void SET0A();
		void SET0B();
		void SET0C();
		void SET0D();
		void SET0E();
		void SET0H();
		void SET0L();
		void SET0HL();
		void SET1A();
		void SET1B();
		void SET1C();
		void SET1D();
		void SET1E();
		void SET1H();
		void SET1L();
		void SET1HL();
		void SET2A();
		void SET2B();
		void SET2C();
		void SET2D();
		void SET2E();
		void SET2H();
		void SET2L();
		void SET2HL();
		void SET3A();
		void SET3B();
		void SET3C();
		void SET3D();
		void SET3E();
		void SET3H();
		void SET3L();
		void SET3HL();
		void SET4A();
		void SET4B();
		void SET4C();
		void SET4D();
		void SET4E();
		void SET4H();
		void SET4L();
		void SET4HL();
		void SET5A();
		void SET5B();
		void SET5C();
		void SET5D();
		void SET5E();
		void SET5H();
		void SET5L();
		void SET5HL();
		void SET6A();
		void SET6B();
		void SET6C();
		void SET6D();
		void SET6E();
		void SET6H();
		void SET6L();
		void SET6HL();
		void SET7A();
		void SET7B();
		void SET7C();
		void SET7D();
		void SET7E();
		void SET7H();
		void SET7L();
		void SET7HL();
	};
}