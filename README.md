# KSharp [BETA] v1.1
The better and clear version of KMLANG


### A Piece of example code

``` ruby
DEBUG OFF

push username(admin)
push password(123)

push try_limit(10)

echo(Password Checker Example)

loop($try_limit)
{
	echo(Enter username : )
	userinput in_name
	echo(Enter password : )
	userinput in_pass

	if($username == $in_name)
	{
		if($password == $in_pass)
		{
			echo(Logined!)
		}
	}


}

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
