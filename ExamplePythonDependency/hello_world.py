def format_name(name: str, max_len: int = 25) -> str:
	return "Hello {}".format(name[:max_len-6].capitalize())