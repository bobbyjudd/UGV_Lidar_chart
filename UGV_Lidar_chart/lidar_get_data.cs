const int GET_NUM = 10;
const int start_step = 180; // 0 degrees. Consider getting info from a text box
const int end_step = 900;// 180 degrees. Consider getting info from a text box
try {
    string port_name;
    int baudrate;
    get_serial_information(out port_name, out baudrate);

    SerialPort urg = new SerialPort(port_name, baudrate);
    urg.NewLine = "\n\n";

    urg.Open();

    urg.Write(SCIP_Writer.SCIP2());
    urg.ReadLine(); // ignore echo back
    urg.Write(SCIP_Writer.MD(start_step, end_step));
    urg.ReadLine(); // ignore echo back

    List<long> distances = new List<long>();
    long time_stamp = 0;
    for (int i = 0; i < GET_NUM; ++i) {
        string receive_data = urg.ReadLine();
        if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances)) {
            Console.WriteLine(receive_data);
            break;
        }
        if (distances.Count == 0) {
            Console.WriteLine(receive_data);
            continue;
        }
        // show distance data
        //Send the distances List to the form graph object
    }

    urg.Write(SCIP_Writer.QT()); // stop measurement mode
    urg.ReadLine(); // ignore echo back
    urg.Close();
} 
catch (Exception ex) {
    Console.WriteLine(ex.Message);
} 
finally {
    Console.WriteLine("Press any key.");
    Console.ReadKey();
}