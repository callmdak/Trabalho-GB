using System;
using System.Collections.Generic;

const int PAGE_SIZE = 8 * 1024;           
const int VIRTUAL_MEMORY_SIZE = 1024 * 1024;
const int PHYSICAL_MEMORY_SIZE = 64 * 1024; 

const int NUM_PAGES = VIRTUAL_MEMORY_SIZE / PAGE_SIZE; 
const int NUM_FRAMES = PHYSICAL_MEMORY_SIZE / PAGE_SIZE; 

MMU mmu = new MMU();

Console.WriteLine("========================================");
Console.WriteLine("SIMULADOR DE MEMÓRIA VIRTUAL");
Console.WriteLine("========================================");
Console.WriteLine($"Memória Virtual: {VIRTUAL_MEMORY_SIZE / 1024} KB");
Console.WriteLine($"Memória Física : {PHYSICAL_MEMORY_SIZE / 1024} KB");
Console.WriteLine($"Páginas        : {NUM_PAGES}");
Console.WriteLine($"Frames         : {NUM_FRAMES}");
Console.WriteLine();

for (int i = 0; i < 20; i++)
{
    int processId = (i % 2) + 1;

    //int virtualAddress = Random.Shared.Next(0, VIRTUAL_MEMORY_SIZE);
    int[] addresses =
    {
        10000,
        12000,
        15000,
        10000,
        12000,
        15000,
        200000,
        210000,
        10000,
        12000,
        300000,
        310000,
        10000,
        12000,
        15000,
        400000,
        500000,
        10000,
        12000,
        15000
    };

    int virtualAddress = addresses[i];

    Console.WriteLine("----------------------------------------");
    Console.WriteLine($"PROCESSO {processId}");
    Console.WriteLine($"Endereço virtual solicitado: {virtualAddress}");

    mmu.Access(processId, virtualAddress);
}

class PageEntry
{
    public bool Present { get; set; } = false;
    public int Frame { get; set; } = -1;
}

class Frame
{
    public bool Free { get; set; } = true;
    public int ProcessId { get; set; } = -1;
    public int PageNumber { get; set; } = -1;
}

class MMU
{
    private const int PAGE_SIZE = 8 * 1024;
    private const int NUM_FRAMES = 8;

    private Dictionary<(int ProcessId, int PageNumber), PageEntry> pageTable =
        new Dictionary<(int, int), PageEntry>();

    private Frame[] frames = new Frame[NUM_FRAMES];

    private Queue<int> fifoQueue = new Queue<int>();

    public MMU()
    {
        for (int i = 0; i < NUM_FRAMES; i++)
        {
            frames[i] = new Frame();
        }
    }

    public void Access(int processId, int virtualAddress)
    {
        int pageNumber = virtualAddress / PAGE_SIZE;
        int offset = virtualAddress % PAGE_SIZE;

        Console.WriteLine($"Página virtual: {pageNumber}");
        Console.WriteLine($"Offset: {offset}");

        if (!pageTable.ContainsKey((processId, pageNumber)))
        {
            pageTable[(processId, pageNumber)] = new PageEntry();
        }

        PageEntry entry = pageTable[(processId, pageNumber)];

        if (!entry.Present)
        {
            Console.WriteLine(">>> PAGE FAULT <<<");

            int frame = FindFrame();

            frames[frame].Free = false;
            frames[frame].ProcessId = processId;
            frames[frame].PageNumber = pageNumber;

            entry.Present = true;
            entry.Frame = frame;

            fifoQueue.Enqueue(frame);

            Console.WriteLine($"Página {pageNumber} carregada no Frame {frame}");
        }
        else
        {
            Console.WriteLine("Página encontrada na memória principal.");
        }

        int physicalAddress = entry.Frame * PAGE_SIZE + offset;

        Console.WriteLine($"Frame utilizado: {entry.Frame}");
        Console.WriteLine($"Endereço físico: {physicalAddress}");

        char content = (char)('A' + (physicalAddress % 26));

        Console.WriteLine($"Conteúdo encontrado: {content}");

        PrintPageTable();
        PrintPhysicalMemory();
    }

    private int FindFrame()
    {
        for (int i = 0; i < NUM_FRAMES; i++)
        {
            if (frames[i].Free)
            {
                Console.WriteLine($"Frame livre encontrado: {i}");
                return i;
            }
        }

        int victimFrame = fifoQueue.Dequeue();

        Console.WriteLine();
        Console.WriteLine("MEMÓRIA CHEIA");
        Console.WriteLine($"Substituindo página {frames[victimFrame].PageNumber} do processo {frames[victimFrame].ProcessId}");

        var victimKey = (
            frames[victimFrame].ProcessId,
            frames[victimFrame].PageNumber
        );

        pageTable[victimKey].Present = false;
        pageTable[victimKey].Frame = -1;

        return victimFrame;
    }

    private void PrintPhysicalMemory()
    {
        Console.WriteLine();
        Console.WriteLine("===== MEMÓRIA FÍSICA =====");

        for (int i = 0; i < NUM_FRAMES; i++)
        {
            if (frames[i].Free)
            {
                Console.WriteLine($"Frame {i} -> LIVRE");
            }
            else
            {
                Console.WriteLine(
                    $"Frame {i} -> Processo {frames[i].ProcessId} | Página {frames[i].PageNumber}"
                );
            }
        }

        Console.WriteLine("==========================");
        Console.WriteLine();
    }

    private void PrintPageTable()
    {
        Console.WriteLine();
        Console.WriteLine("===== TABELA DE PÁGINAS =====");

        foreach (var entry in pageTable)
        {
            int processId = entry.Key.ProcessId;
            int pageNumber = entry.Key.PageNumber;

            if (entry.Value.Present)
            {
                Console.WriteLine(
                    $"Processo {processId} | Página {pageNumber} -> Frame {entry.Value.Frame}"
                );
            }
        }

        Console.WriteLine("=============================");
        Console.WriteLine();
    }
}