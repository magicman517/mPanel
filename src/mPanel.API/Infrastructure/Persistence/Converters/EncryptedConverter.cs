using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace mPanel.API.Infrastructure.Persistence.Converters;

/// <summary>
/// encrypts/decrypts string values using data protection
/// </summary>
public class EncryptedConverter(IDataProtector protector) : ValueConverter<string?, string?>(
    v => v == null ? null : protector.Protect(v),
    v => v == null ? null : protector.Unprotect(v));