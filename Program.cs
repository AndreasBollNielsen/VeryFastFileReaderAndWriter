

using System.Diagnostics;
using System.Runtime.InteropServices;
using VeryFastFileReaderAndWriter;
using static System.Net.Mime.MediaTypeNames;

Stopwatch writeStopwatch = new Stopwatch();
Stopwatch readStopwatch = new Stopwatch();

int arraySize = 50000000;
MyTestStruct[] myStructArray = new MyTestStruct[arraySize];

Console.WriteLine("writing to array");

unsafe
{


    for (long i = 0; i < arraySize; i++)
    {
        // Example data population

        myStructArray[i].id = i + 1;
       // Console.WriteLine("id " + myStructArray[i].id);

        for (long j = 0; j < 8; j++)
        {
            myStructArray[i].temperature[j] = (i + 1) * 2.0f;

           // Console.WriteLine("float input: " + myStructArray[i].temperature[j]);

        }
    }
}





Console.WriteLine("--------------------------------------------------------------------------------------");

string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
string filename = Path.Combine(desktopPath, "myfile.bin");

writeStopwatch.Start(); // Start the stopwatch for writing

// Write the struct array to a file
WriteStructArrayToFile(myStructArray, filename);

writeStopwatch.Stop(); // Stop the stopwatch for writing

Console.WriteLine($"Time taken for writing: {writeStopwatch.ElapsedMilliseconds} milliseconds");

Console.WriteLine("--------------------------------------------------------------------------------------");


readStopwatch.Start(); // Start the stopwatch for reading

// Read the struct array from the file
MyTestStruct[] readTest = ReadBytesToArrayOfStruct(filename);

readStopwatch.Stop(); // Stop the stopwatch for reading

Console.WriteLine($"Time taken for reading from file: {readStopwatch.ElapsedMilliseconds} milliseconds");

// Display the read struct array
unsafe
{
    //foreach (var item in readTest)
    //{
    //    Console.WriteLine($"ID: {item.id}");
    //    Console.WriteLine("Temperature:");
    //    for (int i = 0; i < 8; i++)
    //    {
    //        Console.WriteLine($"  {i + 1}: {item.temperature[i]}");
    //    }
    //    Console.WriteLine(); // Add an empty line for separation
    //}
}

static unsafe void WriteStructArrayToFile(MyTestStruct[] arrayOfstructs, string filename)
{
    // Calculate the total size needed for all structs
    long totalSize = (long)Marshal.SizeOf<MyTestStruct>() * arrayOfstructs.Length;
    long structSize = Marshal.SizeOf<MyTestStruct>();
    byte[] buffer = new byte[totalSize];

    // Extract bytes from each struct and concatenate into one big byte array
    for (long i = 0; i < arrayOfstructs.Length; i++)
    {
        // Calculate the start index in the buffer for the current struct
        long bufferIndex = i * structSize;

        // Copy source array to destination array
        for (long j = 0; j < structSize; j++)
        {
            buffer[bufferIndex + j] = arrayOfstructs[i].byteArray[j];
        }
    }

    // Write the buffer to the file
    using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: false))
    {
        fileStream.Write(buffer, 0, buffer.Length);
    }
}


static unsafe MyTestStruct[] ReadBytesToArrayOfStruct(string filename)
{
    // Calculate the total size needed for all structs
    int structSize = Marshal.SizeOf<MyTestStruct>();
    FileInfo fileInfo = new FileInfo(filename);
    int totalSize = (int)fileInfo.Length;
    int numStructs = totalSize / structSize;

    MyTestStruct[] structs = new MyTestStruct[numStructs];

    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
        long structOffset = 0;
        // Iterate over the byte array and copy bytes into struct instances
        for (int i = 0; i < numStructs; i++)
        {

            // Allocate buffer for reading bytes
            structOffset = 3 * structSize;
            fileStream.Seek(structOffset, SeekOrigin.Begin);
            byte[] buffer = new byte[structSize];

            int bytesRead = fileStream.Read(buffer, 0, structSize);

            // Create a new MyTestStruct instance
            MyTestStruct test = new MyTestStruct();

            // Copy bytes from the buffer to the fixed-size array within the struct
            for (int j = 0; j < bytesRead; j++)
            {
                test.byteArray[j] = buffer[j];
            }

            // Add the struct to the array
            structs[i] = test;
        }
    }

    return structs;
}


