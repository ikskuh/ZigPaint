const std = @import("std");
const GL = @import("gl");

pub fn main() !void {
    const gl = try GL.loadFunctions(GL.GL_1_3);
    std.debug.warn("glGetString: {}\n", gl.getString);

    std.debug.warn("glGetString(GL_VERSION) = {}\n", gl.getString(GL.GL_1_3.VERSION));
}
