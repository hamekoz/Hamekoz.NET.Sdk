﻿namespace Hamekoz.Api.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
