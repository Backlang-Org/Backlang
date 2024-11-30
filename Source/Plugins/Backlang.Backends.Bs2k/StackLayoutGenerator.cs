using System.Diagnostics;

namespace Backlang.Backends.Bs2k;

public class StackLayoutGenerator
{
    private int offset, occupied_stack_space;

    public StackLayoutGenerator(int starting_offset)
    {
        if (starting_offset > 0)
        {
            // alignment is 1, because we assume that the starting offset already has the right alignment
            ClaimStackSpace(starting_offset, 1);
        }
    }

    public int ClaimStackSpace(int size_of_type, int alignment)
    {
        // we have to make sure that the offset has the right alignment
        var old_offset = RoundUp(offset, alignment);
        offset = old_offset + size_of_type;
        if (offset > occupied_stack_space)
        {
            occupied_stack_space = offset;
        }

        return old_offset;
    }

    private int RoundUp(int num_to_round, int multiple)
    {
        // source: https://stackoverflow.com/a/3407254/7540548
        Debug.Assert(multiple > 0);
        var remainder = num_to_round % multiple;

        return remainder == 0 ? num_to_round : num_to_round + multiple - remainder;
    }
}