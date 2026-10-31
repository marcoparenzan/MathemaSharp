# Copyright (c) 2026 Marco Parenzan
# Licensed under the MIT License. See the LICENSE file in the project root.
"""Generate finite binary64 reference samples using Python decimal, not the BCL.
Expected bits must agree at 100 and 200 decimal digits; this is stability evidence,
not a proof that every input or hard-to-round case is correctly rounded.
"""
import decimal
import json
import pathlib
import struct
import sys


def bits(value):
    return struct.pack('>d', value).hex()


def evaluate(name, value, precision):
    with decimal.localcontext() as context:
        context.prec = precision
        number = decimal.Decimal.from_float(value)
        result = {'Sqrt': number.sqrt, 'Exp': number.exp, 'Log': number.ln}[name]()
        return float(result)


def main():
    positive = [5e-324, 2.2250738585072014e-308, 0.25, 0.5,
                0.9999999999999999, 1.0, 1.0000000000000002, 2.0, 4.0,
                10.0, 1000.0, 1e100, sys.float_info.max]
    domains = {'Sqrt': [0.0] + positive, 'Log': positive,
               'Exp': [-745.0, -744.0, -700.0, -100.0, -1.0, -0.25,
                       0.0, 0.25, 1.0, 10.0, 100.0, 700.0, 709.0]}
    cases = []
    for name, values in domains.items():
        for value in values:
            low = bits(evaluate(name, value, 100))
            high = bits(evaluate(name, value, 200))
            if low != high:
                raise RuntimeError(f'Unstable reference: {name}({value!r})')
            cases.append({'function': name, 'inputBits': bits(value), 'expectedBits': high})
    document = {'generator': 'ops/GenerateDoubleOracle.py',
                'reference': 'Python decimal', 'pythonVersion': sys.version.split()[0],
                'decimalVersion': decimal.__version__, 'precisions': [100, 200],
                'acceptanceMaxUlp': 1,
                'scope': 'Finite binary64 samples; no universal accuracy or platform guarantee.',
                'cases': cases}
    output = pathlib.Path(__file__).resolve().parents[1] / 'src/tests/MathemaSharp.Core.Tests/Fixtures/double-elementary-oracle.json'
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(document, indent=2) + '\n', encoding='utf-8')
    print(f'Generated {len(cases)} stable reference cases: {output}')


if __name__ == '__main__':
    main()
