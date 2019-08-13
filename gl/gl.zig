const std = @import("std");
const builtin = @import("builtin");
const so = @import("dynlib.zig");

pub const LoaderError = so.Error || error{
    EntryPointNotFound,
    LibraryNotFound,
};

var libgl: ?so.LibraryHandle = null;

fn ensureLibGL() LoaderError!void {
    // todo: add mutex lock here
    if (libgl) |_| {
        return;
    } else {
        libgl = so.loadLibrary("libGL.so") catch return LoaderError.LibraryNotFound;
    }
}

pub fn loadFunctions(comptime Prototypes: type) LoaderError!Prototypes {
    try ensureLibGL();

    var protos: Prototypes = undefined;
    const info = @typeInfo(Prototypes);
    std.debug.assert(builtin.TypeId(info) == .Struct);

    inline for (info.Struct.fields) |fld| {
        const name = "gl" ++ [_]u8 { std.ascii.toUpper(fld.name[0]) } ++ fld.name[1..];

        const ep = try libgl.?.getEntryPoint(name);

        if(@typeId(fld.field_type) != .Fn) {
            @compileError("All fields in an OpenGL definition must be functions!");
        }

        @field(protos, fld.name) = @ptrCast(fld.field_type, ep);
    }
    return protos;
}

pub const GLuint = c_uint;
pub const GLint = c_int;
pub const GLenum = c_uint;

pub const GL_1_3 = struct {
    bindBuffer: extern fn (target: GLenum, buffer: GLuint) void,
};
