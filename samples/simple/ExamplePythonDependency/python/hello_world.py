def format_name(name: str, max_length: int = 50) -> str:
	return "Hello {}".format(name.capitalize())[:max_length]
