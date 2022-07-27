# KSharp [BETA] v1.1
The better and clear version of KMLANG


### A Piece of example code

``` ruby
DEBUG ON

push INFO(Program ( $GL_VER ) started at $GL_DIRECTORY [ $GL_TIME ])

echo ($INFO)

userinput name

userinput last

echo(Welcome $name $last)

DEBUG OFF

push i(0)

loop(20)
{
	push i(m[($i+1)*2]m)
	echo($i)
}

```

### Used Library to Math Parse : org.mariuszgromada.math.mxparser | http://mathparser.org 
I downloaded from Nuget
