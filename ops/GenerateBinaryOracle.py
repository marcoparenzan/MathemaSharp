# Copyright (c) 2026 Marco Parenzan
# Licensed under the MIT License. See the LICENSE file in the project root.
"""Finite real-domain Pow/LogBase reference samples, not a universal rounding proof."""
import hashlib
import json
import os
import pathlib
import struct
import sys

location = pathlib.Path(os.environ.get('SCIENCE_MPMATH_PATH', r'D:\Dev\BuildArtifacts\tools\mpmath-1.3.0')).resolve()
sys.path.insert(0, str(location))
import mpmath as mp
if mp.__version__ != '1.3.0' or not pathlib.Path(mp.__file__).resolve().is_relative_to(location):
    raise RuntimeError('Pinned external mpmath 1.3.0 required; no global fallback.')


def bits(value):
    return struct.pack('>d', value).hex()


def evaluate(name, x, y, precision):
    with mp.workdps(precision):
        a, b = x.as_integer_ratio(), y.as_integer_ratio()
        a, b = mp.mpf(a[0]) / a[1], mp.mpf(b[0]) / b[1]
        value = mp.power(a, b) if name == 'Pow' else mp.log(a) / mp.log(b)
        return bits(float(value))


def main():
    powers = [(x, y) for x in [0.25, 0.5, 1.0, 2.0, 10.0, 100.0]
              for y in [-3.0, -1.0, 0.5, 1.0, 2.0, 3.0]]
    powers += [(x, y) for x in [-2.0, -10.0] for y in [-3.0, -1.0, 1.0, 2.0, 3.0]]
    logs = [(x, base) for x in [0.25, 0.5, 1.0, 2.0, 4.0, 10.0, 100.0, 1000.0]
            for base in [0.5, 2.0, 4.0, 10.0]]
    cases = []
    for name, pairs in [('Pow', powers), ('LogBase', logs)]:
        for x, y in pairs:
            low, high = evaluate(name, x, y, 160), evaluate(name, x, y, 200)
            if low != high:
                raise RuntimeError(f'Unstable reference: {name}({x},{y})')
            # A mathematically exact zero is canonical +0; signed-zero BCL contracts
            # are tested separately. This is not an approximation reference.
            if name == 'LogBase' and x == 1.0:
                high = bits(0.0)
            cases.append({'function': name, 'inputBits': [bits(x), bits(y)], 'expectedBits': high})
    document = {'generator': 'ops/GenerateBinaryOracle.py', 'reference': 'mpmath',
                'referenceVersion': mp.__version__,
                'referenceModuleSha256': hashlib.sha256(pathlib.Path(mp.__file__).read_bytes()).hexdigest(),
                'pythonVersion': sys.version.split()[0], 'precisions': [160, 200],
                'acceptanceMaxUlp': 1, 'scope': 'Finite real-domain binary64 samples; no universal or platform guarantee.',
                'cases': cases}
    output = pathlib.Path(__file__).resolve().parents[1] / 'src/tests/MathemaSharp.Core.Tests/Fixtures/double-binary-oracle.json'
    output.write_text(json.dumps(document, indent=2) + '\n', encoding='utf-8')
    print(f'Generated {len(cases)} stable reference cases: {output}')


if __name__ == '__main__':
    main()
