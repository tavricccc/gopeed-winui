"""Generate the project's code-authored vector download mark at Windows icon sizes."""
from pathlib import Path
from PIL import Image, ImageDraw

assets = Path(__file__).resolve().parents[1] / 'src' / 'Gopeed.Native' / 'Assets'

def draw_icon(size):
    scale = 4
    image = Image.new('RGBA', (size*scale, size*scale))
    draw = ImageDraw.Draw(image)
    def box(values): return tuple(round(v*size*scale/64) for v in values)
    draw.rounded_rectangle(box((0,0,64,64)), radius=round(size*scale*14/64), fill='#0067C0')
    draw.rounded_rectangle(box((28,11,36,36)), radius=round(size*scale*2/64), fill='white')
    draw.polygon([box((17,30)),box((25,30)),box((32,37)),box((39,30)),box((47,30)),box((32,45))], fill='white')
    draw.rounded_rectangle(box((16,49,48,53)), radius=round(size*scale*2/64), fill='white')
    return image.resize((size,size),Image.Resampling.LANCZOS)

icon = draw_icon(256)
icon.save(assets/'AppIcon.ico',sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
for filename,size in [('Square44x44Logo.scale-200.png',88),('Square44x44Logo.targetsize-24_altform-unplated.png',24),('Square44x44Logo.targetsize-48_altform-lightunplated.png',48),('Square150x150Logo.scale-200.png',300),('StoreLogo.png',50),('LockScreenLogo.scale-200.png',48)]:
    draw_icon(size).save(assets/filename)
print('Generated Windows download icon assets')
