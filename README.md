# CSV Comparison Tool

A simple command-line utility for comparing data within a single CSV file. This tool helps you identify differences between columns or groups of columns quickly.

## Overview

This application runs in a console and guides the user through a series of steps to analyze a CSV file. It's designed for scenarios where you need to reconcile data between different columns, such as comparing two lists or verifying financial data within the same file.

## Features

-   **Two Comparison Modes:**
    1.  **Two-Column Comparison:** Compares any two individual columns.
    2.  **Group Comparison:** Compares two pairs of "ID" and "Amount" columns.
-   **Interactive CLI:** A user-friendly command-line interface that prompts for all necessary information.
-   **Header Support:** Works with CSV files that have or do not have a header row.
-   **Detailed Reports:** Provides a clear summary of the findings directly in the console.

## How to Use

1.  **Run the application** by executing the `.exe` file.
2.  **Enter File Path:** When prompted, provide the full path to the CSV file you want to analyze (e.g., `C:\Users\YourUser\Documents\data.csv`).
3.  **Choose Comparison Type:**
    -   Enter `1` to compare two single columns.
    -   Enter `2` to compare two groups of columns (ID + Amount).
4.  **Configure Columns:**
    -   The tool will display the available columns if a header exists.
    -   Follow the prompts to select the columns you wish to compare by entering their name or number.
5.  **Review Results:** The tool will print a detailed analysis, highlighting:
    -   Mismatched values.
    -   Items that exist in one column/group but not the other.
    -   A final summary of the comparison.
6.  **Run Another Comparison:** After the analysis is complete, you can choose to run another comparison or exit the application.

## Comparison Modes Explained

### 1. Two-Column Comparison

This mode is useful for general-purpose comparison between any two columns (e.g., `Column A` vs `Column B`). It checks for:
-   **Row-level differences:** Identifies rows where the values in the two columns are not the same.
-   **Value existence:** Finds values that appear in one column but are completely absent from the other.
-   **Frequency mismatch:** Reports values that appear a different number of times in each column.

### 2. Group Comparison

This mode is designed for financial reconciliation or similar tasks. It compares two logical groups, where each group consists of an **ID column** and an **Amount column**. For example, you could compare (`Invoice_ID`, `Invoice_Amount`) against (`Paid_ID`, `Paid_Amount`). The analysis identifies:
-   **Amount Mismatches:** IDs that exist in both groups but have different amounts.
-   **Missing IDs:** IDs that are present in one group but not the other.

## Requirements

-   Windows Operating System
-   .NET Framework (usually pre-installed on Windows)
