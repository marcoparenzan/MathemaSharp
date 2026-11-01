# Copyright (c) 2026 Marco Parenzan
# Licensed under the MIT License. See the LICENSE file in the project root.
"""Independent finite binary64 references; precision agreement is not a rounding proof."""
import hashlib
import json
import math
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


def exact(value):
    numerator, denominator = value.as_integer_ratio()
    return mp.mpf(numerator) / denominator


def evaluate(name, inputs, precision):
    with mp.workdps(precision):
        arguments = [exact(value) for value in inputs]
        operation = {'Sin': mp.sin, 'Cos': mp.cos, 'Tan': mp.tan, 'Asin': mp.asin,
                     'Acos': mp.acos, 'Atan': mp.atan, 'Atan2': mp.atan2}[name]
        return bits(float(operation(*arguments)))


def main():
    ordinary = [-1000.0, -10.0, -2.0, -1.0, -0.5, -0.25, -1e-10, 0.0,
                1e-10, 0.25, 0.5, 1.0, 2.0, 10.0, 1000.0, 1e10, 1e100]
    inverse = [-1.0, math.nextafter(-1.0, 0.0), -0.75, -0.5, -0.25, -1e-10,
               0.0, 1e-10, 0.25, 0.5, 0.75, math.nextafter(1.0, 0.0), 1.0]
    cases = []
    domains = {name: [[value] for value in ordinary] for name in ['Sin', 'Cos', 'Tan', 'Atan']}
    domains.update({name: [[value] for value in inverse] for name in ['Asin', 'Acos']})
    domains['Atan2'] = [[y, x] for y in [-2.0, -0.25, 0.25, 2.0] for x in [-2.0, -0.25, 0.25, 2.0]]
    for name, inputs_list in domains.items():
        for inputs in inputs_list:
            low, high = evaluate(name, inputs, 160), evaluate(name, inputs, 200)
            if low != high:
                raise RuntimeError(f'Unstable reference: {name}{inputs}')
            cases.append({'function': name, 'inputBits': [bits(v) for v in inputs], 'expectedBits': high})
    source_hash = hashlib.sha256(pathlib.Path(mp.__file__).read_bytes()).hexdigest()
    document = {'generator': 'ops/GenerateTrigOracle.py', 'reference': 'mpmath',
                'referenceVersion': mp.__version__, 'referenceModuleSha256': source_hash,
                'pythonVersion': sys.version.split()[0], 'precisions': [160, 200],
                'acceptanceMaxUlp': 1,
                'scope': 'Finite exact-input binary64 samples; no universal accuracy or platform guarantee.',
                'cases': cases}
    output = pathlib.Path(__file__).resolve().parents[1] / 'src/tests/MathemaSharp.Core.Tests/Fixtures/double-trig-oracle.json'
    output.write_text(json.dumps(document, indent=2) + '\n', encoding='utf-8')
    print(f'Generated {len(cases)} stable reference cases: {output}')


if __name__ == '__main__':
    main()
