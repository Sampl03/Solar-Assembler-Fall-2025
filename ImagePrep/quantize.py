import pathlib
import numpy as np
from PIL import Image

palette = np.array(Image.open("palette.bmp")).reshape(-1).tolist()
palette_img = Image.new('P', (1, 1))
palette_img.putpalette(palette)

in_dir = pathlib.Path('in')
out_dir = pathlib.Path('out')

for imgfile in in_dir.iterdir():
    if not imgfile.is_file():
        continue

    img_name = imgfile.stem

    image = Image.open(imgfile)\
                 .convert('RGB')\
                 .resize((32, 32), Image.Resampling.NEAREST)
    image = image.quantize(
                 palette = palette_img,
                 method = Image.Quantize.MEDIANCUT,
                 dither=Image.Dither.NONE)

    image.save(out_dir / f"{img_name}.bmp")

    with open(out_dir / f"{img_name}.imgbin", "wb") as binfile:
        array = np.array(image).reshape(-1)
        binfile.write(bytearray(array))