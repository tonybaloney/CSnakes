class Person:
    name: str
    age: int
    height: float
    phobias: list[str]

    def __init__(self, name: str, age: int, height: float, phobias: list[str]):
        self.name = name
        self.age = age
        self.height = height
        self.phobias = phobias

    def scared_of(self, phobia: str) -> bool:
        return phobia in self.phobias

def test_person() -> Person:
    person = Person("John Doe", 30, 5.9, ["spiders", "heights"])
    assert person.name == "John Doe"
    assert person.age == 30
    assert person.height == 5.9
    assert person.phobias == ["spiders", "heights"]
    return person


def test_not_heap() -> Person:
    return "I'm not a heap object"


def test_collection() -> list[Person]:
    person1 = Person("John Doe", 30, 5.9, ["spiders", "heights"])
    person2 = Person("Jane Doe", 25, 5.5, ["snakes"])
    return [person1, person2]
