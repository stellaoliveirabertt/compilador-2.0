using System;
using System.Linq;

namespace MACSLangRuntime
{
    public static class Program
    {
        public static int fatorial(int n)
        {
var resultado = 1            ;
for (int i = 1; i <= n; i = i + 1            )
            {
resultado = resultado * i                ;
            }
return resultado            ;
        }
        
        public static int Main(string[] args)
        {
Console.WriteLine("Digite um número para calcular o fatorial:"            );
var numero            ;
numero = Console.ReadLine(); // Assumindo string ou precisa de conversão específica            
var fat = fatorial(numero)            ;
Console.WriteLine("O fatorial de " + numero + " é " + fat            );
var myFloat = 123.45f            ;
var myChar = 'X'            ;
var myBool = true            ;
if (myBool == false)             {
            }
else             {
var temp = 0                ;
            }
while (myBool)             {
myBool = false                ;
            }
var exprResult = 5 + 3 * 2 == 11 && !false || true            ;
return 0            ;
        }
        
    }
}
