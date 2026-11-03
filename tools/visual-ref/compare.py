"""Diff our WPF metrics against real antd measured in Chrome.

Usage: python compare.py out/metrics.json out/ours.json
Both files carry a "metrics" array of {name, w, h, ...} records.
"""
import json
import re
import sys


def parse_css_color(value):
    if not value or value in ("none", "transparent"):
        return "transparent", 0.0
    m = re.match(r"rgba?\(([^)]+)\)", value)
    if m:
        parts = [p.strip() for p in m.group(1).split(",")]
        r, g, b = (int(round(float(p))) for p in parts[:3])
        a = float(parts[3]) if len(parts) > 3 else 1.0
        return "#%02X%02X%02X" % (r, g, b), a
    m = re.match(r"#([0-9A-Fa-f]{6})@([0-9.]+)", value)
    if m:
        return "#" + m.group(1).upper(), float(m.group(2))
    return value, 1.0


def load(path):
    data = json.load(open(path, encoding="utf-8"))
    out = {}
    for rec in data["metrics"]:
        out[rec["name"]] = rec
    return out


def px(value):
    if isinstance(value, (int, float)):
        return float(value)
    if isinstance(value, str):
        m = re.match(r"-?[\d.]+", value)
        if m:
            return float(m.group(0))
    return None


def main():
    ref = load(sys.argv[1])
    ours = load(sys.argv[2])

    fields = ["w", "h", "paddingLeft", "paddingRight", "fontSize", "fontWeight", "radius"]
    color_fields = ["background", "foreground"]

    print(f"{'component':<20}{'field':<14}{'antd':<12}{'ours':<12}delta")
    print("-" * 62)
    issues = 0
    for name in sorted(ref):
        if name not in ours:
            print(f"{name:<20}{'MISSING':<14}{'':<12}{'':<12}-")
            issues += 1
            continue
        a, b = ref[name], ours[name]
        for field in fields:
            av, bv = px(a.get(field)), px(b.get(field))
            if av is None or bv is None:
                continue
            delta = bv - av
            if abs(delta) > 0.5:
                issues += 1
                print(f"{name:<20}{field:<14}{av:<12g}{bv:<12g}{delta:+g}")
        for field in color_fields:
            ac, aa = parse_css_color(a.get(field, ""))
            bc, ba = parse_css_color(b.get(field, ""))
            if ac != bc or abs(aa - ba) > 0.02:
                issues += 1
                print(f"{name:<20}{field:<14}{ac}@{aa:<8g}{bc}@{ba:<8g}")

    # widths and heights always matter; report the total so CI can gate on it
    print("-" * 62)
    print(f"mismatches: {issues}")
    return 1 if issues else 0


if __name__ == "__main__":
    sys.exit(main())
