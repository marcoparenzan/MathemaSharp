"""Independent reference fixtures. Requires Python 3 and mpmath; no MathemaSharp calls.
Run: python ops/generate-numerical-oracles.py
Decimal strings preserve high precision; binary/decimal consumers round to their own format.
"""
import json
from pathlib import Path
import mpmath as mp

mp.mp.dps = 200

def text(value):
    return mp.nstr(value, 160)

gamma = []
for a, x in [('0.5','0.01'), ('0.5','1'), ('0.5','100'), ('0.5','700'),
             ('2.5','0.1'), ('2.5','10'), ('2.5','700'), ('100','100'),
             ('100','150'), ('100','1000'), ('1000','1200'), ('0.0001','0.0001')]:
    aa, xx = mp.mpf(a), mp.mpf(x)
    gamma.append(dict(a=a, x=x, q=text(mp.gammainc(aa, xx, mp.inf)/mp.gamma(aa))))
loggamma = [dict(x=x, value=text(mp.loggamma(mp.mpf(x)))) for x in
            ['0.0001','0.1','0.5','1.5','10','100','1000']]
hilbert = []
for n in [3,5,8,12]:
    matrix = mp.matrix([[mp.mpf(1)/(i+j+1) for j in range(n)] for i in range(n)])
    rhs = matrix * mp.matrix([1]*n)
    inverse = matrix ** -1
    condition_inf = max(sum(abs(matrix[i,j]) for j in range(n)) for i in range(n)) * max(sum(abs(inverse[i,j]) for j in range(n)) for i in range(n))
    hilbert.append(dict(n=n, matrix=[[text(matrix[i,j]) for j in range(n)] for i in range(n)],
                        rhs=[text(rhs[i]) for i in range(n)], solution=['1']*n,
                        determinant=text(mp.det(matrix)), condition_infinity=text(condition_inf)))
def complex_text(value):
    return [text(mp.re(value)), text(mp.im(value))]

matrix_exp = []
exp_cases = [
    ('diagonal', [[1,0],[0,-1]]),
    ('jordan2', [[-1,20],[0,-1]]),
    ('jordan3', [[-1,20,0],[0,-1,20],[0,0,-1]]),
    ('stiff_jordan', [[-5,100],[0,-5]]),
    ('complex_rotation', [[0,-mp.j],[ -mp.j,0]]),
]
for name, entries in exp_cases:
    matrix = mp.matrix(entries)
    exponential = mp.expm(matrix)
    matrix_exp.append(dict(name=name, matrix=[[complex_text(matrix[i,j]) for j in range(matrix.cols)] for i in range(matrix.rows)],
                          expected=[[complex_text(exponential[i,j]) for j in range(matrix.cols)] for i in range(matrix.rows)]))
hermitian = []
for name, entries in [
    ('diagonal_imbalance', [[mp.mpf('1'),0],[0,mp.mpf('1e-40')]]),
    ('coupled_imbalance', [[1,mp.mpf('1e-20')],[mp.mpf('1e-20'),mp.mpf('2e-40')]]),
    ('complex_hermitian', [[2,1+2*mp.j],[1-2*mp.j,3]]),
]:
    matrix = mp.matrix(entries)
    eigenvalues, _ = mp.eighe(matrix)
    hermitian.append(dict(name=name, matrix=[[complex_text(matrix[i,j]) for j in range(2)] for i in range(2)],
                          eigenvalues=[text(eigenvalues[1]),text(eigenvalues[0])]))

output = dict(generator='mpmath; no MathemaSharp implementation used', mpmath_version=mp.__version__,
              precision_decimal_digits=mp.mp.dps, stored_significant_digits=160,
              gamma_q=gamma, log_gamma=loggamma, hilbert=hilbert, matrix_exp=matrix_exp, hermitian=hermitian)
path = Path(__file__).resolve().parent / 'oracles' / 'numerical-reference.json'
path.parent.mkdir(parents=True, exist_ok=True)
path.write_text(json.dumps(output, indent=2)+'\n', encoding='utf-8')
print(f'Wrote {path}; mpmath {mp.__version__}, {mp.mp.dps} digits')
