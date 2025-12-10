using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Code;
using Solar.Asm.Engine.Model.Symbols;

namespace Demo.Mos6502
{
    public class Mos6502Program(SharedMetaContext sharedMeta) : Program(sharedMeta)
    {
        public override Section CreateOrGetSection(string name, string configString)
        {
            var existingSection = CodeEntities
                .SearchEntities<Section>()
                .Where(s => s.Name == name)
                .OrderBy(s => s.DeclarationOrder)
                .FirstOrDefault();

            if (existingSection is not null)
                return existingSection;

            // config string is unused here for simplicity
            Section newSection = new(
                name, 
                new()
                {
                    Alignment = 0,
                    PadValue = [0x00], // Pad with BRK
                    IsMergeable = true, // Merge sections across files
                    IsData = true,
                    IsUnitialised = false,
                    IsExecutable = true,
                    IsWriteable = true
                }
            );
            newSection.Initialise(CodeEntities);

            // Create a symbol referencing this
            Symbol newSectionSymbol = new(new([]), name);
            newSectionSymbol.Initialise(Symbols);
            newSectionSymbol.DefineAsSection(newSection);

            return newSection;
        }
    }
}
