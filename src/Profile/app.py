def generate_data(a: int, b: str, c: tuple[float, str]) -> list[dict[str, tuple[int, float]]]:
    return [
        {
            b: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         },
         {
            c[1]: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         }
        ]
