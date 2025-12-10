using Demo.Mos6502;
using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.Expressions;
using Solar.Asm.Engine.Model.Symbols;

namespace Demo
{
    /// <summary>
    /// Class building the image demo program in lieu of text input with a parser
    /// </summary>
    public static class Demo6502Image
    {
        public static void ReadDemoProgram(Program program)
        {
            //=================//
            // DEFINE SECTIONS //
            //=================//
            Section codeSection = program.CreateOrGetSection("code", "");
            codeSection.DesiredAddress = 0x600;
            codeSection.IsDesiredAddressFixed = true;

            Section imagesSection = program.CreateOrGetSection("images", "");
            imagesSection.DesiredAddress = 0x6a8; 
            imagesSection.IsDesiredAddressFixed = true;


            //=============//
            // LOAD IMAGES //
            //=============//
            ulong numImages = 0;
            foreach (string imgfile in Directory.GetFiles("Images/"))
            {
                if (Path.GetExtension(imgfile).ToLower() != ".imgbin")
                    continue;

                // Make one fragment per image
                Fragment imageFragment = imagesSection.CreateFragment();
                Chunk imgChunk = imageFragment.IncBin(imgfile);
                if (imgChunk.CalculateMemSize() != 32 * 32)
                    throw new SmlaException($"Loaded image '{Path.GetFileName(imgfile)}' must be {32 * 32} bytes long");

                numImages++;
            }

            Symbol symAbsNUMIMAGES = program.CreateOrGetSymbol("NUM_IMAGES"); // Number of images
            symAbsNUMIMAGES.DefineAsAbsolute(numImages);


            //===================//
            // DECLARE CONSTANTS //
            //===================//
            // Screen spans addresses 0200-05FF, but it's easier for us to hardcode rather than using a symbol

            Symbol symAddrKEYPRESS = program.CreateOrGetSymbol("KEYPRESS");
            symAddrKEYPRESS.DefineAsAbsolute(0xFF); // Address of latest keypress

            Symbol symAbsNEXT = program.CreateOrGetSymbol("ASCII_NEXT");
            symAbsNEXT.DefineAsAbsolute(0x64); // D key

            Symbol symAbsPREV = program.CreateOrGetSymbol("ASCII_PREV");
            symAbsPREV.DefineAsAbsolute(0x61); // A key

            Symbol symAbsSTOP = program.CreateOrGetSymbol("ASCII_STOP");
            symAbsSTOP.DefineAsAbsolute(0x73); // S key

            // The two following variables in the zero page are used to access screen memory and image memory
            // Their upper byte is incremented by 1 every time a page has been written
            Symbol symVarIMGADDR = program.CreateOrGetSymbol("IMAGE_ADDRESS");
            symVarIMGADDR.DefineAsAbsolute(0x01); // Address of image address variable (since it's 16-bits)

            Symbol symVarSCREENADDR = program.CreateOrGetSymbol("SCREEN_ADDRESS");
            symVarSCREENADDR.DefineAsAbsolute(0x03); // Address of screen address variable (since it's 16-bits)


            //======================//
            // DECLARE CODE SYMBOLS //
            //======================//
            Symbol lbl_quit = program.CreateOrGetSymbol("quit");
            Symbol lbl_init = program.CreateOrGetSymbol("init");
            Symbol lbl_loop = program.CreateOrGetSymbol("loop");
            Symbol lbl_clearScreen = program.CreateOrGetSymbol("clearScreen");
            Symbol symSecImages = program.CreateOrGetSymbol("images"); // Should have been defined when `images` section was created
            Symbol lbl_nextImage = program.CreateOrGetSymbol("nextImage");
            Symbol lbl_prevImage = program.CreateOrGetSymbol("prevImage");
            Symbol lbl_drawImageJump = program.CreateOrGetSymbol("drawImageJump");
            Symbol lbl_stopImage = program.CreateOrGetSymbol("stopImage");
            Symbol lbl_drawImage = program.CreateOrGetSymbol("drawImage");
            Symbol lbl_offset = program.CreateOrGetSymbol("offset");
            Symbol lbl_end_offset = program.CreateOrGetSymbol("end_offset");
            Symbol lbl_draw_loop = program.CreateOrGetSymbol("draw_loop");
            Symbol lbl_end_draw = program.CreateOrGetSymbol("end_draw");
            Symbol lbl_clear_loop = program.CreateOrGetSymbol("clear_loop");


            //=================//
            // DECLARE OPCODES //
            //=================//
            byte BRK_IMPL = Mos6502InstructionTable.TryFindOpcode("BRK", Mos6502AddressMode.AccImplied)!.Value;

            byte JMP_ABS = Mos6502InstructionTable.TryFindOpcode("JMP", Mos6502AddressMode.Absolute)!.Value;
            byte JSR_ABS = Mos6502InstructionTable.TryFindOpcode("JSR", Mos6502AddressMode.Absolute)!.Value;
            byte RTS_IMPL = Mos6502InstructionTable.TryFindOpcode("RTS", Mos6502AddressMode.AccImplied)!.Value;

            byte LDA_IMM = Mos6502InstructionTable.TryFindOpcode("LDA", Mos6502AddressMode.Immediate)!.Value;
            byte LDA_ZPG = Mos6502InstructionTable.TryFindOpcode("LDA", Mos6502AddressMode.ZeroPage)!.Value;
            byte LDA_INDY = Mos6502InstructionTable.TryFindOpcode("LDA", Mos6502AddressMode.IndirectYZpg)!.Value;
            byte LDX_IMM = Mos6502InstructionTable.TryFindOpcode("LDX", Mos6502AddressMode.Immediate)!.Value;
            byte LDY_IMM = Mos6502InstructionTable.TryFindOpcode("LDY", Mos6502AddressMode.Immediate)!.Value;
            byte STA_ZPG = Mos6502InstructionTable.TryFindOpcode("STA", Mos6502AddressMode.ZeroPage)!.Value;
            byte STA_INDY = Mos6502InstructionTable.TryFindOpcode("STA", Mos6502AddressMode.IndirectYZpg)!.Value;
            byte STY_ZPG = Mos6502InstructionTable.TryFindOpcode("STY", Mos6502AddressMode.ZeroPage)!.Value;

            byte TAY_IMPL = Mos6502InstructionTable.TryFindOpcode("TAY", Mos6502AddressMode.AccImplied)!.Value;
            byte TYA_IMPL = Mos6502InstructionTable.TryFindOpcode("TYA", Mos6502AddressMode.AccImplied)!.Value;

            byte PHA_IMPL = Mos6502InstructionTable.TryFindOpcode("PHA", Mos6502AddressMode.AccImplied)!.Value;
            byte PLA_IMPL = Mos6502InstructionTable.TryFindOpcode("PLA", Mos6502AddressMode.AccImplied)!.Value;

            byte ADC_IMM = Mos6502InstructionTable.TryFindOpcode("ADC", Mos6502AddressMode.Immediate)!.Value;

            byte INC_ZPG = Mos6502InstructionTable.TryFindOpcode("INC", Mos6502AddressMode.ZeroPage)!.Value;
            byte INY_IMPL = Mos6502InstructionTable.TryFindOpcode("INY", Mos6502AddressMode.AccImplied)!.Value;
            byte DEX_IMPL = Mos6502InstructionTable.TryFindOpcode("DEX", Mos6502AddressMode.AccImplied)!.Value;
            byte DEY_IMPL = Mos6502InstructionTable.TryFindOpcode("DEY", Mos6502AddressMode.AccImplied)!.Value;

            byte CMP_IMM = Mos6502InstructionTable.TryFindOpcode("CMP", Mos6502AddressMode.Immediate)!.Value;
            byte CPX_IMM = Mos6502InstructionTable.TryFindOpcode("CPX", Mos6502AddressMode.Immediate)!.Value;
            byte CPY_IMM = Mos6502InstructionTable.TryFindOpcode("CPY", Mos6502AddressMode.Immediate)!.Value;

            byte CLC_IMPL = Mos6502InstructionTable.TryFindOpcode("CLC", Mos6502AddressMode.AccImplied)!.Value;

            byte BCC_REL = Mos6502InstructionTable.TryFindOpcode("BCC", Mos6502AddressMode.Relative)!.Value;
            byte BEQ_REL = Mos6502InstructionTable.TryFindOpcode("BEQ", Mos6502AddressMode.Relative)!.Value;
            byte BMI_REL = Mos6502InstructionTable.TryFindOpcode("BMI", Mos6502AddressMode.Relative)!.Value;
            byte BNE_REL = Mos6502InstructionTable.TryFindOpcode("BNE", Mos6502AddressMode.Relative)!.Value;
            byte BPL_REL = Mos6502InstructionTable.TryFindOpcode("BPL", Mos6502AddressMode.Relative)!.Value;


            //===================//
            // IMPLEMENT PROGRAM //
            //===================//
            Fragment codeFragment = codeSection.CreateFragment();

            //   jsr init
            //   jsr drawImage
            //   jsr loop
            //   jsr clearScreen
            // quit:
            //   brk
            {
                Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_init)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_drawImage)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_loop)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_clearScreen)).RegisterToFragment(codeFragment);

                lbl_quit.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(BRK_IMPL).RegisterToFragment(codeFragment),
                    0);
            }

            //
            // init:
            //   lda #NUM_IMAGES
            //   cmp #0
            //   beq quit
            //   ldy #0                     ; Set current image to 0. Y tracks current image
            //   sty KEYPRESS               ; Reset keypress just in case
            //   sty SCREEN_ADDRESS         ; Lower byte of screen address won't change, it's always 0, so pre-store
            //   lda #<$ images & 0xFF $>   ; Lower byte of image address won't change since images are 0x400 bytes long, so pre-store
            //   sta IMAGE_ADDRESS
            //   rts
            {
                lbl_init.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(LDA_IMM, SymbolRefExpr.From(program, symAbsNUMIMAGES)).RegisterToFragment(codeFragment),
                    0);

                Mos6502Chunk.CreateImmediate(CMP_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_quit)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDY_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(STY_ZPG, SymbolRefExpr.From(program, symAddrKEYPRESS)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(STY_ZPG, SymbolRefExpr.From(program, symVarSCREENADDR)).RegisterToFragment(codeFragment);

                var images_lower = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symSecImages),
                    LiteralExpr<ulong>.FromValue(program, 0xFF),
                    SymbolMath.BitwiseAnd
                );
                Mos6502Chunk.CreateImmediate(LDA_IMM, images_lower).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(STA_ZPG, SymbolRefExpr.From(program, symVarIMGADDR)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImplied(RTS_IMPL).RegisterToFragment(codeFragment);
            }

            // loop:
            //   lda KEYPRESS       ; Read the latest key and check if it matches a command
            //   cmp #ASCII_NEXT
            //   beq nextImage
            //   cmp #ASCII_PREV
            //   beq prevImage
            //   cmp #ASCII_STOP
            //   beq stopImage
            //   jmp loop           ; Loop until we get a valid key
            {
                lbl_loop.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(LDA_ZPG, SymbolRefExpr.From(program, symAddrKEYPRESS)).RegisterToFragment(codeFragment),
                    0);

                Mos6502Chunk.CreateImmediate(CMP_IMM, SymbolRefExpr.From(program, symAbsNEXT)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_nextImage)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(CMP_IMM, SymbolRefExpr.From(program, symAbsPREV)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_prevImage)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(CMP_IMM, SymbolRefExpr.From(program, symAbsSTOP)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_stopImage)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JMP_ABS, SymbolRefExpr.From(program, lbl_loop)).RegisterToFragment(codeFragment);
            }

            // nextImage:
            //   iny
            //   cpy #NUM_IMAGES    ; Wrap around to zero if greater than or equal to NUM_IMAGES
            //   bmi drawImageJump
            //   ldy #0
            //   beq drawImageJump  ; Unconditional branch
            //
            // prevImage:
            //   dey
            //   bpl drawImageJump       ; Wrap around to NUM_IMAGES if less than 0
            //   ldy <$ #NUM_IMAGES-1 $>
            //  
            // drawImageJump:
            //   jsr drawImage
            //   jmp loop
            //
            // stopImage:
            //   rts                ; Exit to clearScreen
            {
                lbl_nextImage.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(INY_IMPL).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImmediate(CPY_IMM, SymbolRefExpr.From(program, symAbsNUMIMAGES)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BMI_REL, SymbolRefExpr.From(program, lbl_drawImageJump)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDY_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_drawImageJump)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_drawImage)).RegisterToFragment(codeFragment);

                lbl_prevImage.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(DEY_IMPL).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateRelative(BPL_REL, SymbolRefExpr.From(program, lbl_drawImage)).RegisterToFragment(codeFragment);
                var numimgm1_expr = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symAbsNUMIMAGES),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Sub
                );
                Mos6502Chunk.CreateImmediate(LDY_IMM, numimgm1_expr).RegisterToFragment(codeFragment);

                lbl_drawImageJump.DefineAsLabel(
                    Mos6502Chunk.CreateAddress(JSR_ABS, SymbolRefExpr.From(program, lbl_drawImage)).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateAddress(JMP_ABS, SymbolRefExpr.From(program, lbl_loop)).RegisterToFragment(codeFragment);

                lbl_stopImage.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(RTS_IMPL).RegisterToFragment(codeFragment),
                    0);
            }

            // drawImage:
            //   tya                        ; Save current image (Y) to stack
            //   pha                        ;
            //   lda #0				        ; Set keypress to 0 to avoid flashing
            //   sta KEYPRESS
            //   lda #2                     ; Reset the screen address, it's always 2
            //   sta <$ SCREEN_ADDRESS+1 $>
            //   lda #<$ images >> 8 $>     ; Reset the image address. Get upper byte of images section into A
            {
                lbl_drawImage.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(TYA_IMPL).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImplied(PHA_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDA_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(STA_ZPG, SymbolRefExpr.From(program, symAddrKEYPRESS)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDA_IMM, LiteralExpr<ulong>.FromValue(program, 2)).RegisterToFragment(codeFragment);

                var screenaddr_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symVarSCREENADDR),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Add
                );
                Mos6502Chunk.CreateImmediate(STA_ZPG, screenaddr_upper).RegisterToFragment(codeFragment);

                var images_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symSecImages),
                    LiteralExpr<ulong>.FromValue(program, 8),
                    SymbolMath.ShiftRight
                );
                Mos6502Chunk.CreateImmediate(LDA_IMM, images_upper).RegisterToFragment(codeFragment);
            }

            //   cpy #0                 ; Address of image is 0x400*Y + (IMAGE_ADDRESS), so we count down, adding 4 to A
            //   beq end_offset
            // offset: 
            //   adc #4
            //   dey
            //   bne offset
            // end_offset:
            //   sta <$ IMAGE_ADDRESS+1 $>  ; Store the upper byte now that it points to the correct image
            {
                Mos6502Chunk.CreateImmediate(CPY_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BEQ_REL, SymbolRefExpr.From(program, lbl_end_offset)).RegisterToFragment(codeFragment);

                lbl_offset.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(CLC_IMPL).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImmediate(ADC_IMM, LiteralExpr<ulong>.FromValue(program, 4)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImplied(DEY_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BNE_REL, SymbolRefExpr.From(program, lbl_offset)).RegisterToFragment(codeFragment);

                var imgaddr_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symVarIMGADDR),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Add
                );
                lbl_end_offset.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(STA_ZPG, imgaddr_upper).RegisterToFragment(codeFragment),
                    0);
            }

            //   ldx #4                     ; Now we need to copy the 4 pages per image, using X to track
            //   ldy #0                     ; And we use Y as the index into the page
            // draw_loop:
            //   lda (IMAGE_ADDRESS),Y      ; Load from image through Y-offset indirection
            //   sta (SCREEN_ADDRESS),Y     ; Store to screen space through same Y-offset indirection
            //   iny                        ; Increment Y.
            //   bne draw_loop              ; If it didn't roll over to 0, continue on same page
            //   inc <$ IMAGE_ADDRESS+1 $>  ; Otherwise, increase page for address variables and decrease X
            //   inc <$ SCREEN_ADDRESS+1 $>
            //   dex
            //   bne draw_loop              ; If X hasn't reached 0, we still have more pages to draw
            // end_draw:
            //   pla                        ; Pull current image back from stack into Y
            //   tay
            //   jmp loop
            {
                Mos6502Chunk.CreateImmediate(LDX_IMM, LiteralExpr<ulong>.FromValue(program, 4)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDY_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);

                lbl_draw_loop.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(LDA_INDY, SymbolRefExpr.From(program, symVarIMGADDR)).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImmediate(STA_INDY, SymbolRefExpr.From(program, symVarSCREENADDR)).RegisterToFragment(codeFragment);

                Mos6502Chunk.CreateImplied(INY_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BNE_REL, SymbolRefExpr.From(program, lbl_draw_loop)).RegisterToFragment(codeFragment);

                var imgaddr_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symVarIMGADDR),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Add
                );
                Mos6502Chunk.CreateImmediate(INC_ZPG, imgaddr_upper).RegisterToFragment(codeFragment);

                var screenaddr_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symVarSCREENADDR),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Add
                );
                Mos6502Chunk.CreateImmediate(INC_ZPG, screenaddr_upper).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImplied(DEX_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BNE_REL, SymbolRefExpr.From(program, lbl_draw_loop)).RegisterToFragment(codeFragment);

                lbl_end_draw.DefineAsLabel(
                    Mos6502Chunk.CreateImplied(PLA_IMPL).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImplied(TAY_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateAddress(JMP_ABS, SymbolRefExpr.From(program, lbl_loop)).RegisterToFragment(codeFragment);
            }

            // clearScreen:
            //   lda #2                     ; Reset screen address to 0200
            //   sta <$ SCREEN_ADDRESS+1 $>
            //   lda #0                     ; Just iterate over screen space, writing 0
            //   ldx #4
            //   ldy #0
            // clear_loop:
            //   sta (SCREEN_ADDRESS),Y
            //   iny
            //   bne clear_loop
            //   inc <$ SCREEN_ADDRESS+1 $>
            //   dex
            //   bne clear_loop
            //   rts
            //
            {
                lbl_clearScreen.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(LDA_IMM, LiteralExpr<ulong>.FromValue(program, 2)).RegisterToFragment(codeFragment),
                    0);

                var screenaddr_upper = BinaryExpr<ulong, ulong, ulong>.From(
                    program,
                    SymbolRefExpr.From(program, symVarSCREENADDR),
                    LiteralExpr<ulong>.FromValue(program, 1),
                    SymbolMath.Add
                );
                Mos6502Chunk.CreateImmediate(STA_ZPG, screenaddr_upper).RegisterToFragment(codeFragment);

                Mos6502Chunk.CreateImmediate(LDA_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDX_IMM, LiteralExpr<ulong>.FromValue(program, 4)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(LDY_IMM, LiteralExpr<ulong>.FromValue(program, 0)).RegisterToFragment(codeFragment);

                lbl_clear_loop.DefineAsLabel(
                    Mos6502Chunk.CreateImmediate(STA_INDY, SymbolRefExpr.From(program, symVarSCREENADDR)).RegisterToFragment(codeFragment),
                    0);
                Mos6502Chunk.CreateImplied(INY_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BNE_REL, SymbolRefExpr.From(program, lbl_clear_loop)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImmediate(INC_ZPG, screenaddr_upper).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImplied(DEX_IMPL).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateRelative(BNE_REL, SymbolRefExpr.From(program, lbl_clear_loop)).RegisterToFragment(codeFragment);
                Mos6502Chunk.CreateImplied(RTS_IMPL).RegisterToFragment(codeFragment);
            }

        }

        //===================//
        // EXTENSION METHODS //
        //===================//
        static Chunk RegisterToFragment(this Chunk chunk, Fragment fragment)
        {
            chunk.Initialise(fragment.OwningProgram.CodeEntities);
            fragment.AddChunk(chunk);
            return chunk;
        }

        static Chunk DefineByte(this Fragment fragment, byte value) => fragment.DefineBytes([value]);
        static Chunk DefineBytes(this Fragment fragment, byte[] values)
        {
            fragment.GuardValidity();

            Chunk chunk = new BinaryChunk(values, []);
            chunk.Initialise(fragment.OwningProgram.CodeEntities);
            fragment.AddChunk(chunk);

            return chunk;
        }

        static Chunk DefineAddress(this Fragment fragment, ushort address) => fragment.DefineAddresses([address]);
        static Chunk DefineAddresses(this Fragment fragment, ushort[] addresses)
        {
            byte[] values = addresses.SelectMany(address => fragment.OwningProgram.ArchSpecs.AddressToBytes(address)).ToArray();
            return fragment.DefineBytes(values);
        }

        static Chunk IncBin(this Fragment fragment, string filename) => fragment.DefineBytes(File.ReadAllBytes(filename));
    }
}
