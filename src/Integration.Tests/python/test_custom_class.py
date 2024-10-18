class Person:
    def __init__(self, first_name, last_name):
        self.first_name = first_name
        self.last_name = last_name
    def full_name(self):
        return f"{self.first_name} {self.last_name}"

def create_person(first_name: str, last_name: str) -> Person:
    return Person(first_name, last_name)
