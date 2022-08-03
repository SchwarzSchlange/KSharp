# KSharp [BETA] v1.1
The better and clear version of KMLANG


### A Piece of example code

``` ruby
DEBUG OFF
TITLE(ADMIN ENTER)
LROM 0



loop(100)
{
	echo_a(Admin Name : )
	userinput gName


	if($gName == admin)
	{
		echo_a(Admin Password : )
		userinput gPass
		if($gPass == 123)
		{
			BREAK
		}
		else
		{
			CLEARSC
			echo(Password is false)
		}
	}
	else
	{
		CLEARSC
		echo(Username is false)
	}

}

CLEARSC
echo(Successfully Logined...)


```

``` ruby
DEBUG ON

block sum(a,b)
{

	echo(m[$a+$b]m)

}


loop(999)
{
	userinput x
	userinput y
	
	call topla($x,$y)
}

```

``` ruby
DEBUG OFF

block isPrime(number)
{
	if(m[ispr($number)]m == 1)
	{
		echo("$number" is a prime number)
	}

}

echo(Prime number finder by Kaan Temizkan)

echo(------------------)

echo(Enter a limit : )
userinput limit

push i(1)

loop($limit)
{
	call isPrime($i);	
	push i(m[$i+1]m)
}

```


### Used Library to Math Parse : org.mariuszgromada.math.mxparser | http://mathparser.org 
I downloaded from Nuget
