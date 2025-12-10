using Demo.Mos6502;
using Solar.Asm.Engine.Model;
using Solar.Asm.Engine.Model.Exceptions;
using Solar.Asm.Engine.Model.IO;

namespace Demo.CLI
{
    internal class Frontend
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(
                """
                +-----------------------------------------------------+
                |  ___  ________ _____    ____  _____  _____  _____   |
                |  |  \/  |  _  /  ___|  / ___||  ___||  _  |/ __  \  |
                |  | .  . | | | \ `--.  / /___ |___ \ | |/' |`' / /'  |
                |  | |\/| | | | |`--. \ | ___ \    \ \|  /| |  / /    |
                |  | |  | \ \_/ /\__/ / | \_/ |/\__/ /\ |_/ /./ /___  |
                |  \_|  |_/\___/\____/  \_____/\____/  \___/ \_____/  |
                |                                                     |
                |____________________________________________________/
                |                                                    |
                |               Assembly Demonstration               |
                |___________________________________________________/

                """
            );

            try
            {
                // Create directories if they don't exist
                Directory.CreateDirectory("Images/");
                Directory.CreateDirectory("out/");
                Assemble();
            }
            catch (SmlaException e)
            {
                Console.WriteLine("ERROR !\n");
                Console.WriteLine(e.ToString());
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("NON-SMLA ERROR !\n");
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public static void Assemble()
        {
            /*
             * Create the main program objects
             */
            Console.Write("Creating meta-context... ");

            IAssemblyParser dialect = new Mos6502Parser();
            SharedMetaContext sharedMeta = Easy6502Formatter.Create(dialect);
            Program mainProgram = sharedMeta.Outputter.CreateProgram();

            Console.WriteLine("DONE");


            /*
             * Pass it off to the input reader standing
             */
            Console.Write("Building demo program... ");

            Demo6502Image.ReadDemoProgram(mainProgram);

            Console.WriteLine("DONE");



            /*
             * Output to temporary file
             */
            string tempFile = Path.GetTempFileName();
            try
            {
                Console.Write("Assembling program... ");

                using (FileStream tempFs = new(tempFile, FileMode.Create, FileAccess.Write))
                    sharedMeta.Outputter.EmitProgram(mainProgram, tempFs);

                Console.WriteLine("DONE");

                /*
                 * Copy on success
                 */

                Console.Write("Writing to 'out/demo6502_image.o'... ");

                try { Directory.CreateDirectory("out/"); } catch { }
                File.Copy(tempFile, "out/demo6502_image.o", true);

                Console.WriteLine("DONE");
            } 
            catch { throw; }
            finally
            {
                /// Cleanup the temporary file
                try { File.Delete(tempFile); } catch { }
            }

            Console.WriteLine("\nDemo assembled successfully. Quitting.");
        }
    }
}


