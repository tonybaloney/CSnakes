def get_mad_string() -> str:
    return (
        'A'            # ASCII (1 byte)
        '¢'            # U+00A2 (2 bytes)
        'ह'            # U+0939 (3 bytes)
        '𝄞'            # U+1D11E (4 bytes, musical symbol)
        '🦋'           # U+1F98B (4 bytes, butterfly emoji)
        '�'            # U+FFFD (replacement character)
        '\u0301'       # U+0301 (combining acute accent)
        'ñ'            # U+00F1 (precomposed accented letter)
        '\u202E'       # U+202E (right-to-left override)
        '🏳️‍🌈'           # Emoji sequence: white flag + ZWJ + rainbow
    )
