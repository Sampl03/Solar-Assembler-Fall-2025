with open("demo6502_image.o", "rb") as fin:
    with open("demo6502_image.s", "w") as fout:
        while (len(line := fin.read(16)) > 0):
            fout.write("DCB ")
            line = [f"${byte:02X}" for byte in line]
            fout.write(", ".join(line) + "\n")