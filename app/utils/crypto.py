"""
Cryptography utilities for PodMD.

Provides encryption/decryption functions for sensitive data using Fernet (AES128).
"""

from cryptography.fernet import Fernet

from app.config import settings


def encrypt_token(plaintext: str) -> str:
    """
    Encrypt a plaintext token using Fernet symmetric encryption.

    Args:
        plaintext: The plaintext token to encrypt

    Returns:
        Base64-encoded encrypted token string
    """
    key = settings.encryption_key.encode()
    f = Fernet(key)
    encrypted = f.encrypt(plaintext.encode())
    return encrypted.decode()


def decrypt_token(encrypted: str) -> str:
    """
    Decrypt an encrypted token using Fernet symmetric encryption.

    Args:
        encrypted: Base64-encoded encrypted token string

    Returns:
        The decrypted plaintext token
    """
    key = settings.encryption_key.encode()
    f = Fernet(key)
    decrypted = f.decrypt(encrypted.encode())
    return decrypted.decode()
