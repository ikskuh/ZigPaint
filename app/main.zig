const std = @import("std");
const GL = @import("gl");

pub fn main() !void {
    const gl = try GL.loadFunctions(GL.GL_1_3);
    std.debug.warn("glBindBuffer: {}\n", gl.bindBuffer);
}
