const std = @import("std");
const builtin = @import("builtin");

pub const Error = error{
    LibraryNotFound,
    EntryPointNotFound,
    OutOfMemory,
};

const impl = comptime switch (builtin.os) {
    .linux => struct {
        const Handle = *@OpaqueType();

        const RTLD_LAZY = 1;
        const RTLD_NOW = 2;

        extern fn dlopen(filename: [*c]const u8, flags: c_int) ?Handle;
        extern fn dlsym(handle: Handle, symbol: [*c]const u8) ?*c_void;
        extern fn dlclose(handle: Handle) c_int;

        fn getEntryPoint(handle: Handle, name: []const u8) Error!Symbol {
            var buffer = [_]u8{0} ** 512;
            var alloca = std.heap.FixedBufferAllocator.init(buffer[0..]);
            var zstr = try alloca.allocator.alloc(u8, name.len + 1);
            std.mem.copy(u8, zstr, name);
            zstr[name.len] = 0;
            return dlsym(handle, zstr.ptr) orelse return Error.EntryPointNotFound;
        }

        fn loadLibrary(name: []const u8) Error!Handle {
            var buffer = [_]u8{0} ** 512;
            var alloca = std.heap.FixedBufferAllocator.init(buffer[0..]);
            var zstr = try alloca.allocator.alloc(u8, name.len + 1);
            std.mem.copy(u8, zstr, name);
            zstr[name.len] = 0;
            return dlopen(zstr.ptr, RTLD_LAZY) orelse return Error.LibraryNotFound;
        }
    },
    .windows => struct {},
    else => @compileError("OS not supported for shared-object!"),
};

pub const LibraryHandle = struct {
    handle: impl.Handle,

    fn getEntryPoint(self: @This(), name: []const u8) Error!Symbol {
        return impl.getEntryPoint(self.handle, name);
    }
};

pub const Symbol = *c_void;

pub fn loadLibrary(name: []const u8) Error!LibraryHandle {
    return LibraryHandle{
        .handle = try impl.loadLibrary(name),
    };
}
