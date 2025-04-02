def scream(letter: str, length: int) -> str:
    return letter * length

def scream_names(names: list[str], length: int) -> list[str]:
    return [scream(name, length) for name in names]

