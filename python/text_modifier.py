"""Die eigentlichen Textmodifikationen.

Wendet die aktiven Modifikationen der Reihe nach auf einen Eingabetext an.
"""

import threading

import ascii_art


class ModificationSettings:
    """Thread-sicherer Zustand, welche Textmodifikationen aktuell aktiv sind.

    Wird von der Webseite (HTTP) gesetzt und vom UDP-Handler gelesen.
    """

    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._upper_case = False
        self._camel_case = False
        self._ascii_art = False

    def snapshot(self):
        """Liefert eine konsistente Momentaufnahme aller Flags."""
        with self._lock:
            return (self._upper_case, self._camel_case, self._ascii_art)

    def update(self, upper_case=None, camel_case=None, ascii_art=None):
        """Setzt einzelne Flags (None = unveraendert lassen)."""
        with self._lock:
            if isinstance(upper_case, bool):
                self._upper_case = upper_case
            if isinstance(camel_case, bool):
                self._camel_case = camel_case
            if isinstance(ascii_art, bool):
                self._ascii_art = ascii_art

    def as_dict(self):
        u, c, a = self.snapshot()
        return {"upperCase": u, "camelCase": c, "asciiArt": a}


def to_upper_case(text: str) -> str:
    """Wandelt den gesamten Text in GROSSBUCHSTABEN um."""
    return text.upper()


def to_camel_case(text: str) -> str:
    """Wandelt den Text in CaMeLcAsE um: jeder zweite Buchstabe gross bzw. klein.

    Gezaehlt werden nur Buchstaben - Leer- und Sonderzeichen unterbrechen das
    Muster nicht.
    """
    result = []
    letter_index = 0
    for ch in text:
        if ch.isalpha():
            result.append(ch.upper() if letter_index % 2 == 0 else ch.lower())
            letter_index += 1
        else:
            result.append(ch)
    return "".join(result)


def to_ascii_art(text: str) -> str:
    """Wandelt den Text in mehrzeilige ASCII-Art (Blockschrift) um."""
    return ascii_art.render(text)


def apply(text: str, settings: ModificationSettings) -> str:
    """Wendet alle aktivierten Modifikationen an.

    Reihenfolge: 1. GROSSBUCHSTABEN, 2. CaMeLcAsE, 3. ASCII-Art.
    Ist keine Modifikation aktiv, wird der Text unveraendert zurueckgegeben.
    """
    upper, camel, ascii_on = settings.snapshot()
    result = text

    if upper:
        result = to_upper_case(result)
    if camel:
        result = to_camel_case(result)
    if ascii_on:
        result = to_ascii_art(result)

    return result
