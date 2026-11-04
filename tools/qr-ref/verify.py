"""Decodes the matrices our encoder produces, using a real scanner.

The project has no third-party runtime dependencies, so the encoder is hand written; this
script is the reference check and is not part of the build.

    python -m pip install zxing-cpp pillow
    dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --qr-dump out/
    python tools/qr-ref/verify.py out/

Full matrix equality against another encoder is not a valid test: the same text may use a
different mode (byte vs alphanumeric) or mask and still be a correct symbol. Decoding the
rendered modules is what actually proves the encoder.
"""

import os
import sys

import zxingcpp
from PIL import Image


def decode(rows, scale=8, quiet=4):
    size = len(rows)
    image = Image.new("L", ((size + 2 * quiet) * scale, (size + 2 * quiet) * scale), 255)
    pixels = image.load()

    for row, line in enumerate(rows):
        for col, value in enumerate(line):
            if value != "#":
                continue
            for y in range((row + quiet) * scale, (row + quiet + 1) * scale):
                for x in range((col + quiet) * scale, (col + quiet + 1) * scale):
                    pixels[x, y] = 0

    found = zxingcpp.read_barcodes(image)
    return found[0].text if found else None


def main(directory):
    failures = 0
    for name in sorted(os.listdir(directory)):
        if not name.endswith(".txt"):
            continue

        path = os.path.join(directory, name)
        lines = [line.rstrip("\n") for line in open(path, encoding="utf-8")]
        expected = lines[0]
        actual = decode(lines[1:])

        status = "PASS" if actual == expected else "FAIL"
        if status == "FAIL":
            failures += 1
        print("%s  %-28s expected=%r decoded=%r" % (status, name, expected, actual))

    print("\n%d failure(s)" % failures)
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1] if len(sys.argv) > 1 else "."))
